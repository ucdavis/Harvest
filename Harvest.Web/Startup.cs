using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Apm.NetCoreAll;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Harvest.Email.Services;
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
using EmailBodyService = Harvest.Email.Services.EmailBodyService;
using IEmailBodyService = Harvest.Email.Services.IEmailBodyService;

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
            });

            // define accessCode => roleCode[] mapping once for both AccessHandler and UserService
            static string[] GetRoles(string accessCode)
            {
                return accessCode switch
                {
                    // System can access anything
                    AccessCodes.SystemAccess => new[] { Role.Codes.System },
                    // FieldManager can access anything restricted to FieldManager, Supervisor, Worker or PI roles
                    AccessCodes.FieldManagerAccess => new[] { Role.Codes.FieldManager },
                    // Supervisor can access anything restricted to Supervisor, Worker or PI roles
                    AccessCodes.SupervisorAccess => new[] { Role.Codes.Supervisor, Role.Codes.FieldManager },
                    // Worker can access anything restricted to Worker role
                    AccessCodes.WorkerAccess => new[] { Role.Codes.Worker, Role.Codes.Supervisor, Role.Codes.FieldManager },
                    // PI can access anything restricted to PI role
                    AccessCodes.PrincipalInvestigator => new[] { Role.Codes.PI, Role.Codes.Supervisor, Role.Codes.FieldManager },
                    _ => throw new ArgumentException($"{nameof(accessCode)} is not a valid {nameof(AccessCodes)} constant")
                };
            }

            services.AddAuthorization(options =>
            {
                // no need to specify additional roles for system admin, as an exception is made for it in VerifyRoleAccessHandler
                options.AddPolicy(AccessCodes.SystemAccess, policy => policy.Requirements.Add(new VerifyRoleAccess(GetRoles(AccessCodes.SystemAccess))));
                options.AddPolicy(AccessCodes.FieldManagerAccess, policy => policy.Requirements.Add(new VerifyRoleAccess(GetRoles(AccessCodes.FieldManagerAccess))));
                options.AddPolicy(AccessCodes.SupervisorAccess, policy => policy.Requirements.Add(new VerifyRoleAccess(GetRoles(AccessCodes.SupervisorAccess))));
                options.AddPolicy(AccessCodes.WorkerAccess, policy => policy.Requirements.Add(new VerifyRoleAccess(GetRoles(AccessCodes.WorkerAccess))));
                options.AddPolicy(AccessCodes.PrincipalInvestigator, policy => policy.AddRequirements(new VerifyRoleAccess(GetRoles(AccessCodes.PrincipalInvestigator))));
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

            services.AddSingleton<IFileService, FileService>();
            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddScoped<ISlothService, SlothService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailBodyService, EmailBodyService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped(provder => JsonOptions.Standard);
            services.AddScoped<IProjectHistoryService, ProjectHistoryService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddTransient<RoleResolver>(serviceProvider => GetRoles);
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
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<LogUserNameMiddleware>();
            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new { controller = "(invoice|rate|permissions)" }
                );

                endpoints.MapControllerRoute(
                    name: "Projects",
                    pattern: "{controller=Home}/{action=Index}/{projectId?}");
            });

            // SPA needs to kick in for all paths during development
            // TODO: create SPA 404 page or have SPA redirect back to MVC app on invalid route
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
