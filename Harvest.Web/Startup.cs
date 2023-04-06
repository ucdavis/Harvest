using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Apm.NetCoreAll;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Harvest.Web.Handlers;
using Harvest.Web.Middleware;
using Harvest.Web.Models;
using Harvest.Web.Models.Settings;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Harvest.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<SerilogControllerActionFilter>();
            }).AddJsonOptions(options => options.JsonSerializerOptions.WithStandard().WithGeoJson());

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(oidc =>
            {
                oidc.ClientId = Configuration["Authentication:ClientId"];
                oidc.ClientSecret = Configuration["Authentication:ClientSecret"];
                oidc.Authority = Configuration["Authentication:Authority"];
                oidc.ResponseType = OpenIdConnectResponseType.Code;
                oidc.Scope.Add("openid");
                oidc.Scope.Add("profile");
                oidc.Scope.Add("email");
                oidc.Scope.Add("eduPerson");
                oidc.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                };
                oidc.Events.OnTicketReceived = async context => {
                    var identity = (ClaimsIdentity)context.Principal.Identity;
                    if (identity == null)
                    {
                        return;
                    }

                    // Sometimes CAS doesn't return the required IAM ID
                    // If this happens, we take the reliable Kerberos (NameIdentifier claim) and use it to lookup IAM ID
                    if (!identity.HasClaim(c => c.Type == "ucdPersonIAMID"))
                    {
                        var identityService = context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();
                        var kerbId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (kerbId != null)
                        {
                            var identityUser = await identityService.GetByKerberos(kerbId.Value);

                            if (identityUser != null)
                            {
                                identity.AddClaim(new Claim("ucdPersonIAMID", identityUser.Iam));
                            }
                        }
                    }

                    // Ensure user exists in the db
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    await userService.GetUser(identity.Claims.ToArray());
                };
            });

            services.AddAuthorization(options =>
            {
                // no need to specify additional roles for system admin, as an exception is made for it in VerifyRoleAccessHandler
                options.AddPolicy(AccessCodes.SystemAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.SystemAccess))));
                options.AddPolicy(AccessCodes.FieldManagerAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.FieldManagerAccess))));
                options.AddPolicy(AccessCodes.SupervisorAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.SupervisorAccess))));
                options.AddPolicy(AccessCodes.WorkerAccess, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.WorkerAccess))));
                options.AddPolicy(AccessCodes.PrincipalInvestigator, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.PrincipalInvestigator))));
                options.AddPolicy(AccessCodes.PrincipalInvestigatorOnly, policy => policy.Requirements.Add(
                    new VerifyRoleAccess(AccessConfig.GetRoles(AccessCodes.PrincipalInvestigatorOnly))));
            });

            services.AddScoped<IAuthorizationHandler, VerifyRoleAccessHandler>();


            // setup entity framework
            // "Provider" config only present when using ef migrations cli
            var efProvider = Configuration.GetValue("Provider", "none");

            if (efProvider == "SqlServer" || (efProvider == "none" && Configuration.GetValue<bool>("Dev:UseSql")))
            {
                services.AddDbContextPool<AppDbContext, AppDbContextSqlServer>((serviceProvider, o) =>
                {
                    o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                        sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly("Harvest.Core");
                            sqlOptions.UseNetTopologySuite();
                        });
#if DEBUG
                    o.EnableSensitiveDataLogging();
#endif
                });
            }
            else
            {
                services.AddDbContextPool<AppDbContext, AppDbContextSqlite>((serviceProvider, o) =>
                {
                    var connection = new SqliteConnection("Data Source=harvest.db");
                    o.UseSqlite(connection, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly("Harvest.Core");
                        sqliteOptions.UseNetTopologySuite();
                    });
                });
            }

            services.Configure<AuthSettings>(Configuration.GetSection("Authentication"));
            services.Configure<FinancialLookupSettings>(Configuration.GetSection("FinancialLookup"));
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));
            services.Configure<SparkpostSettings>(Configuration.GetSection("SparkPost"));
            services.Configure<StorageSettings>(Configuration.GetSection("Storage"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<DevSettings>(Configuration.GetSection("Dev"));
            services.Configure<AggieEnterpriseOptions>(Configuration.GetSection("AggieEnterprise"));

            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<RewriteError404>();
            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddScoped<ISlothService, SlothService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped(provider => JsonOptions.Standard);
            services.AddScoped<IProjectHistoryService, ProjectHistoryService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddTransient<RoleResolver>(serviceProvider => AccessConfig.GetRoles);
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IAggieEnterpriseService, AggieEnterpriseService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
        {
            app.UseAllElasticApm(Configuration);

            ConfigureDb(dbContext);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    // cache our static assest, i.e. CSS and JS, for a long time
                    if (context.Context.Request.Path.Value.StartsWith("/static"))
                    {
                        var headers = context.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(365)
                        };
                    }
                }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<LogUserNameMiddleware>();
            app.UseSerilogRequestLogging();
            
            app.UseEndpoints(endpoints =>
            {
                // team routes for server-side endpoints
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/{team}/{controller}/{action}/{id?}",
                    defaults: new { action = "Index" },
                    constraints: new { controller = "(help|rate|permissions)" }
                );
                
                // default for MVC server-side endpoints
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new { controller = "(account|crop|home|system|help|error|report)" }
                );

                // API routes map to all other controllers
                endpoints.MapControllerRoute(
                    name: "API",
                    pattern: "/api/{controller=Project}/{action=Index}/{projectId?}");

                // any other nonfile route should be handled by the spa, except leave the sockjs route alone if we are in dev mode (hot reloading)
                if (env.IsDevelopment()) {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { path = new RegexRouteConstraint("^(?!sockjs-node).*$") }
                    );
                } else {
                    endpoints.MapControllerRoute(
                        name: "react",
                        pattern: "{*path:nonfile}",
                        defaults: new { controller = "Home", action = "Index" }
                    );
                }
            });

            // SPA needs to kick in for all paths during development
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        private void ConfigureDb(AppDbContext dbContext)
        {
            var recreateDb = Configuration.GetValue<bool>("Dev:RecreateDb");

            if (recreateDb)
            {
                dbContext.Database.EnsureDeleted();
            }

            dbContext.Database.Migrate();


            var initializer = new DbInitializer(dbContext);
            initializer.Initialize(recreateDb).GetAwaiter().GetResult();

        }
    }
}
