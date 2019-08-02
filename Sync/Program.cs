using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Sync.Service;

namespace Sync
{
    class Program
    {

        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole(LogEventLevel.Verbose, "{Timestamp:HH:mm:ss} {Level:u3} {Message}{NewLine}{Exception}")
                .CreateLogger();

            // DI

            var services = new ServiceCollection()
                .AddLogging(i =>
                {
                    i.AddConsole();
                    i.AddDebug();
                })
                .AddSingleton<ISyncService, SyncService>()
                ;

            // Configuration Loader

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", true, true)
                .Build()
                ;

            // Pass configuration to services
            services.Configure<SyncServiceOptions>(config.GetSection("syncService"));



            var serviceProvider = services.BuildServiceProvider();



            var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            Log.Information("Starting application");

            //do the actual work here
            var bar = serviceProvider.GetService<ISyncService>();

            bar.Start();

            Log.Information("End");

            Console.ReadKey();
        }
    }
}
