using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sync.Service;

namespace Sync
{
    class Program
    {
        static void Main(string[] args)
        {

            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ISyncService, SyncService>();

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", true, true)
                .Build();

       
            services.Configure<SyncServiceOptions>(config.GetSection("syncService"));


            var serviceProvider = services.BuildServiceProvider();



            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            logger.LogDebug("Starting application");

            //do the actual work here
            var bar = serviceProvider.GetService<ISyncService>();
            bar.DoSync();

            logger.LogDebug("All done!");

            Console.ReadKey();
        }
    }
}
