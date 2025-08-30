using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using YTMediaControllerSrv.Controller;
using YTMediaControllerSrv.Logging;
using YTMediaControllerSrv.Types;


namespace YTMediaControllerSrv.Server
{
    internal class UISocketServer
    {
        public WebSocketConnectionManager wsManager;
        private int backendServerPort;
        private ILogger Logger;
        public UISocketServer(string host, int port, int backendServerPort, ILogger logger)
        {
            this.backendServerPort = backendServerPort;
            this.Logger = logger;
            string endpoint = $"http://{host}:{port}/";

            wsManager = new WebSocketConnectionManager(endpoint, logger);

            wsManager.OnMessage += OnMessage;
            wsManager.OnConnect += OnConnected;
            wsManager.OnDisconnect += OnDisconnected;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                await wsManager.StartAsync();
            });
        }

        public void Stop()
        {
            wsManager.Stop();
        }

        private void OnConnected()
        {
            Logger.Info($"Client connected");
        }

        private void OnDisconnected()
        {
            Logger.Info($"Client disconnected");
        }

        public async Task Send(object jsonObject)
        {
            if (wsManager.IsConnected())
            {
                await wsManager.SendAsync(jsonObject);
            }
            else
            {
                Logger.Warn("[ControlServer] Cannot send: No client is connected.");
            }
        }

        public void SendSync(object jsonObject)
        {
            Task.Run(async () =>
            {
                await Send(jsonObject);
            });
        }

        public void OnMessage(string jsonString)
        {
            var obj = JsonConvert.DeserializeObject<UISocketMessage>(jsonString);

            switch (obj.Action)
            {
                case "webPlaybackStarted":
                    {
                        SystemController.TriggerYoutubeFullsceen();
                        break;
                    }
                case "enforcementDialogRemoved":
                    {
                        SystemController.TriggerYoutubePlay();
                        break;
                    }
            }
        }
    }
}