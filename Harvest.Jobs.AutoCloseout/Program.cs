using System;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Harvest.Jobs.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace Harvest.Jobs.AutoCloseout
{
    class Program : JobBase
    {
        private static ILogger _log;
        static void Main(string[] args)
        {
            Configure();
            var assembyName = typeof(Program).Assembly.GetName();
            _log = Log.Logger
                .ForContext("jobname", assembyName.Name)
                .ForContext("jobid", Guid.NewGuid());

            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);
            var provider = ConfigureServices();

            var emailService = provider.GetService<IEmailService>();
            var invoiceService = provider.GetService<IInvoiceService>();

            ProcessProjectsNeedingAutoCloseout(invoiceService).GetAwaiter().GetResult();

            ProcessNotifications(emailService).GetAwaiter().GetResult();
        }

        private static async Task ProcessProjectsNeedingAutoCloseout(IInvoiceService invoiceService)
        {
            try
            {
                var count = await invoiceService.AutoCloseoutProjects(-18);
                Log.Information($"Projects Auto Approved for closeout: {count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.Message);
            }
        }

        private static async Task ProcessNotifications(IEmailService emailService)
        {
            try
            {
                var count = await emailService.ResendCloseoutNotifications();
                Log.Information($"Projects awaiting closeout confirmation resent: {count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.InnerException.Message);
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            //TODO: Have a SQL Lite option?
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

            services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<FinancialLookupSettings>(Configuration.GetSection("FinancialLookup"));
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));
            services.Configure<DevSettings>(Configuration.GetSection("Dev"));

            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<IProjectHistoryService, ProjectHistoryService>(); //Don't know if I need this or the user stuff.
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton(provder => JsonOptions.Standard);
            services.AddSingleton<IHttpContextAccessor, NullHttpContextAccessor>();
            services.AddTransient<RoleResolver>(serviceProvider => AccessConfig.GetRoles);
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<ISlothService, SlothService>();
            services.AddTransient<IDateTimeService, DateTimeService>();


            return services.BuildServiceProvider();
        }
    }
}

