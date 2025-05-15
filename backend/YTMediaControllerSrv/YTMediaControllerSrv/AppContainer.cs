using System;
using YTMediaControllerSrv.Server;
using YTMediaControllerSrv.Settings;

namespace YTMediaControllerSrv
{
    internal class AppContainer
    {
        private ControlServer controlServer;
        private BackendServer backendServer;
        private PlaybackManager playbackManager;

        public AppContainer() {
            string settingsFile = PathResolver.GetSettingsFilePath();

            string deviceIP = DeviceInfo.GetLocalIPAddress();
            AppSettingsJson settings = new AppSettings(settingsFile).settings;

            controlServer = new ControlServer("localhost", settings.BackgroundServerPort + 1);
            playbackManager = new PlaybackManager(controlServer);
            backendServer = new BackendServer(deviceIP, settings.BackgroundServerPort, playbackManager);

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
            controlServer.Start();
            backendServer.Start();
        }

        public void Stop()
        {
            controlServer.Stop();
            backendServer.Stop();
        }
    }
}
