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

            var app = new AppContainer();
            app.Start();
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
