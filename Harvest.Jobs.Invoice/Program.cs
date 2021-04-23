using System;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Core.Utilities;
using Harvest.Jobs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Harvest.Jobs.Invoice
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

            // setup di
            var provider = ConfigureServices();

            var invoiceService = provider.GetService<IInvoiceService>();
            var slothService = provider.GetService<ISlothService>();

            ProcessInvoices(invoiceService, slothService).GetAwaiter().GetResult();
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

            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));
            services.Configure<FinancialLookupSettings>(Configuration.GetSection("FinancialLookup"));

            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ISlothService, SlothService>();
            services.AddScoped<IFinancialService, FinancialService>();
            //services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));
            services.AddScoped(provder => StandardJsonOptions.GetOptions());

            return services.BuildServiceProvider();
        }

        private static async Task ProcessInvoices(IInvoiceService invoiceService, ISlothService slothService)
        {
           var invoiceCount = await invoiceService.CreateInvoices();
            _log.Information("Harvest Invoices Created: {invoiceCount}", invoiceCount);

            var slothMoneyMoveCount = 0;
            var createdInvoices = await invoiceService.GetCreatedInvoiceIds();
            if (createdInvoices != null && createdInvoices.Count > 0)
            {
                _log.Information("Processing {count} invoices", createdInvoices.Count);
                foreach (var createdInvoice in createdInvoices)
                {
                    var response = await slothService.MoveMoney(createdInvoice);
                    if (response == null)
                    {
                        _log.Information("Invoice not found. Id: {createdInvoice}", createdInvoice);
                        continue;
                    }

                    if (!response.Success)
                    {
                        _log.Information("Invoice error. Id: {createdInvoice}, Error: {error}", createdInvoice, response.Message);
                    }

                    slothMoneyMoveCount++;
                }
                _log.Information("Money Moved Invoices: {slothMoneyMoveCount}", slothMoneyMoveCount);
            }

            await slothService.ProcessTransferUpdates();
        }
    }
}
