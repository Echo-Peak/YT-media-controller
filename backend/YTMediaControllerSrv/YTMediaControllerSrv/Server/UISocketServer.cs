using Newtonsoft.Json;
using System.Threading.Tasks;
using YTMediaControllerSrv.Logging;
using YTMediaControllerSrv.Settings;
using YTMediaControllerSrv.Types;


namespace YTMediaControllerSrv.Server
{
    internal class UISocketServer
    {
        public WebSocketConnectionManager wsManager;
        private int backendServerPort;
        private ILogger Logger;
        private readonly AppSettings appSettings;
        public UISocketServer(string host, AppSettings settings, ILogger logger)
        {
            this.backendServerPort = settings.BackendServerPort;
            this.appSettings = settings;
            this.Logger = logger;
            string endpoint = $"http://{host}:{settings.UISocketServerPort}/";

            wsManager = new WebSocketConnectionManager(endpoint, logger);

            wsManager.OnMessageNs += OnMessage;
            wsManager.OnConnectNs += OnConnected;
            wsManager.OnDisconnectNs += OnDisconnected;
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

        private void OnConnected(WSNamespace ns)
        {
            Logger.Info($"Client connected in namespace: \"{ns.Value}\"");
        }

        private void OnDisconnected(WSNamespace ns)
        {
            Logger.Info($"Client disconnected from namespace: \"{ns.Value}\"");
        }

        public async Task Send(WSNamespace ns, object jsonObject)
        {
            if (wsManager.IsConnected())
            {
                await wsManager.SendAsync(ns, jsonObject);
            }
            else
            {
                Logger.Warn($"[ControlServer] Cannot send: No client is connected in the namespace \"{ns.Value}\"");
            }
        }

        public void SendSync(WSNamespace ns, object jsonObject)
        {
            Task.Run(async () =>
            {
                await Send(ns, jsonObject);
            });
        }

        public void OnMessage(WSNamespace ns, string jsonString)
        {
            var obj = JsonConvert.DeserializeObject<UISocketMessage>(jsonString);

            switch (obj.Action)
            {
                case "getBackendSettings":
                    {
                        SendSync(ns, new
                        {
                            Action = "backendSettings",
                            Data = new
                            {
                                DeviceNetworkIp = DeviceInfo.GetLocalIPAddress(),
                                appSettings.BackendServerPort,
                                appSettings.UISocketServerPort
                            }
                        });
                        break;
                    }
            }
        }
    }
}