using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YTMediaControllerSrv;

namespace YTMediaControllerUpdaterSrv
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

#if WINDOWS
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "YTMediaControllerUpdaterService";
            });
#else
            builder.Services.AddSystemd();
#endif
            builder.Services.AddHostedService<Service1>();

            var host = builder.Build();

#if DEBUG
            StartCliApp();
#else
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
