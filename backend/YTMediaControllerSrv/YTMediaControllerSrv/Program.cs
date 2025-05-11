using System;
using System.Collections.Generic;
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
            string deviceIP = DeviceInfo.GetLocalIPAddress();
            var controlServer = new ControlServer(deviceIP ,45020);
            var playbackManager = new PlaybackManager(controlServer);
            new BackendServer(deviceIP, 45021, playbackManager);
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
