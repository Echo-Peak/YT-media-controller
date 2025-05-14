using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Settings;

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

            string settingsFile = PathResolver.GetSettingsFilePath();

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            AppSettingsJson settings = new AppSettings(settingsFile).settings;

            var controlServer = new ControlServer();
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
