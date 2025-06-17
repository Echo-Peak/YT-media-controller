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
        private BrowserExtensionNativeHost nativeHost;

        public AppContainer() {
            string settingsFile = PathResolver.GetSettingsFilePath();
            GlobalFFOptions.Configure(opt => opt.BinaryFolder = PathResolver.GetFFMpegDir());

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            AppSettingsJson settings = new AppSettings(settingsFile).settings;

            uiSocketServer = new UISocketServer("localhost", settings.BackgroundServerPort + 1, settings.BackgroundServerPort);
            backendServer = new BackendServer(deviceIP, settings.BackgroundServerPort, uiSocketServer);
           // nativeHost = new BrowserExtensionNativeHost();



#if DEBUG
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("SIGINT received. Cleaning up resources...");
                Stop();
                Console.WriteLine("Cleanup complete. Exiting application.");
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
