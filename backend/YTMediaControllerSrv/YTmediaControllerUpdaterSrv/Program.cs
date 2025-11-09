using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YTMediaControllerSrv;

namespace YTMediaControllerUpdaterSrv
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "YTMediaControllerUpdaterService";
            });
            builder.Services.AddHostedService<Service1>();

            var host = builder.Build();

#if DEBUG
            StartCliApp();
#else
            // Run as Windows Service in release mode
            host.Run();
#endif
        }
        
        static void StartCliApp()
        {
            var logger = new Logger("AUTO_UPDATER");
            var updaterApi = new UpdaterApi();
            var updater = new Updater(logger, updaterApi);

            Task.Run(async () =>
            {
                await updater.CheckForUpdate();
            }).Wait();
        }
    }
}
