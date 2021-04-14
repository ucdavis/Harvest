using System;
using Harvest.Core.Data;
using Harvest.Core.Services;
using Harvest.Jobs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harvest.Jobs.Invoice
{
    class Program : JobBase
    {
        //private static ILogger _log;
        static void Main(string[] args)
        {
            Configure();
            var assembyName = typeof(Program).Assembly.GetName();
            //_log = Log.Logger
            //    .ForContext("jobname", assembyName.Name)
            //    .ForContext("jobid", Guid.NewGuid());

            //_log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);


            // setup di
            var provider = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddDbContextPool<AppDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IInvoiceService, InvoiceService>();
            //services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));

            return services.BuildServiceProvider();
        }
    }
}
