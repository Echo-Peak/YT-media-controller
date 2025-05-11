using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Server;

namespace YTMediaControllerSrv
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            RunAsConsoleApp();
            Console.ReadKey();
#else
            RunAsService();
#endif
        }
        static void RunAsConsoleApp()
        {
            // This requires app to be running as admin to bind port

            var installDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
            string settingsFile = Path.Combine(installDir, "settings.json");

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            AppSettings settings = new AppSettings(settingsFile);
            var controlServer = new ControlServer(deviceIP, settings.ControlServerPort);
            var playbackManager = new PlaybackManager(controlServer);
            var backendServer = new BackendServer(deviceIP, settings.BackgroundServerPort, playbackManager);

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("SIGINT received. Cleaning up resources...");
                backendServer.Stop();
                controlServer.Stop();
                Console.WriteLine("Cleanup complete. Exiting application.");
            };

            controlServer.Start();
            backendServer.Start();
        }

        static void RunAsService()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
