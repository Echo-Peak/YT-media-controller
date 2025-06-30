using FFMpegCore;
using Newtonsoft.Json;
using System;
using System.IO;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Settings;
using YTMediaControllerSrv.Types;

namespace YTMediaControllerSrv
{
    internal class AppContainer
    {
        private UISocketServer uiSocketServer;
        private BackendServer backendServer;

        public AppContainer() {
            string settingsFile = PathResolver.GetSettingsFilePath();
            GlobalFFOptions.Configure(opt => opt.BinaryFolder = PathResolver.GetFFMpegDir());

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            AppSettingsJson settings = new AppSettings(settingsFile).settings;

            uiSocketServer = new UISocketServer("localhost", settings.UISocketServerPort, settings.BackgroundServerPort);
            backendServer = new BackendServer(deviceIP, settings.BackgroundServerPort, uiSocketServer);



#if DEBUG
            Console.CancelKeyPress += (sender, e) =>
            {
                Logger.Info("SIGINT received. Cleaning up resources...");
                Stop();
                Logger.Info("Cleanup complete. Exiting application.");
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
            uiSocketServer.Stop();
            backendServer.Stop();
        }
    }
}
