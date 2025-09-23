using FFMpegCore;
using Newtonsoft.Json;
using System;
using System.IO;
using YTMediaControllerSrv.Logging;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Settings;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class AppContainer
    {
        private UISocketServer uiSocketServer;
        private BackendServer backendServer;
        public ILogger defaultLogger = new Logger();
        private FirewallManager firewallManager;

        public AppContainer() {
            var appSettings = new AppSettings(defaultLogger);
            firewallManager = new FirewallManager(defaultLogger);
            firewallManager.Update("YTMediaControllerBackendServerRule", appSettings.BackendServerPort);

            GlobalFFOptions.Configure(opt => opt.BinaryFolder = PathResolver.GetFFMpegDir());

            string deviceIP = DeviceInfo.GetLocalIPAddress();

            uiSocketServer = new UISocketServer("localhost", appSettings.UISocketServerPort, appSettings.BackendServerPort, defaultLogger);
            backendServer = new BackendServer(deviceIP, appSettings.BackendServerPort, uiSocketServer, defaultLogger);



#if DEBUG
            Console.CancelKeyPress += (sender, e) =>
            {
                defaultLogger.Info("SIGINT received. Cleaning up resources...");
                Stop();
                defaultLogger.Info("Cleanup complete. Exiting application.");
            };
#endif
        }
        public void Start()
        {
            uiSocketServer.Start();
            backendServer.Start();
        }

        public void Stop()
        {
            firewallManager.Remove("YTMediaControllerBackendServerRule");
            uiSocketServer.Stop();
            backendServer.Stop();
        }
    }
}
