using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace YTMediaControllerSrv
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
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
            // Run as Windows Service in release mode
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "YTMediaControllerService";
            });
            builder.Services.AddHostedService<Service1>();

            var host = builder.Build();
            host.Run();
#endif
        }
    }
}
