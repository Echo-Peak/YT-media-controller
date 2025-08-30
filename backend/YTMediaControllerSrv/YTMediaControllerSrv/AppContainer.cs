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

        public AppContainer() {
            string settingsFile = PathResolver.GetSettingsFilePath();
            GlobalFFOptions.Configure(opt => opt.BinaryFolder = PathResolver.GetFFMpegDir());

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            var appSettings = new AppSettings(settingsFile, defaultLogger);
            var settingsJson = appSettings.Load();

            uiSocketServer = new UISocketServer("localhost", settingsJson.UISocketServerPort, settingsJson.BackendServerPort, defaultLogger);
            backendServer = new BackendServer(deviceIP, settingsJson.BackendServerPort, uiSocketServer, defaultLogger);



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
            uiSocketServer.Stop();
            backendServer.Stop();
        }
    }
}
