using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YTMediaControllerSrv
{
    internal static class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            // Run as console app in debug mode
            var app = new AppContainer();
            app.Start();
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();
            app.Stop();
#else
            var builder = Host.CreateApplicationBuilder(args);

#if WINDOWS
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "YTMediaControllerService";
            });
#else
            builder.Services.AddSystemd();
#endif
            builder.Services.AddHostedService<Service1>();

            var host = builder.Build();
            host.Run();
#endif
        }
    }
}
