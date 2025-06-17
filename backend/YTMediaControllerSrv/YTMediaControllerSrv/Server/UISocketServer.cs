using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using YTMediaControllerSrv.Controller;
using YTMediaControllerSrv.Types;


namespace YTMediaControllerSrv.Server
{
    internal class UISocketServer
    {
        public WebSocketConnectionManager wsManager;
        private int backendServerPort;
        public UISocketServer(string host, int port, int backendServerPort)
        {
            this.backendServerPort = backendServerPort;
            string endpoint = $"http://{host}:{port}/";

            wsManager = new WebSocketConnectionManager(endpoint);

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
            Console.WriteLine($"Client connected");
        }

        private void OnDisconnected()
        {
            Console.WriteLine($"Client disconnected");
        }

        public async Task Send(object jsonObject)
        {
            if (wsManager.IsConnected())
            {
                await wsManager.SendAsync(jsonObject);
            }
            else
            {
                Console.Error.WriteLine("[ControlServer] Cannot send: No client is connected.");
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
            var obj = JsonConvert.DeserializeObject<NamedPipeMessage>(jsonString);
            switch (obj.Action)
            {
                case "playbackStarted":
                    {
                        SystemController.EnterFullScreen();
                        break;
                    }
                case "getDeviceInfo":
                    {
                        var deviceInfo = new
                        {
                            action = "deviceInfo",
                            data = new { 
                                devicePort = backendServerPort,
                                deviceIp = DeviceInfo.GetLocalIPAddress()
                            }
                        };
                        SendSync(deviceInfo);
                        break;
                    
                    }
            }
        }
    }
}