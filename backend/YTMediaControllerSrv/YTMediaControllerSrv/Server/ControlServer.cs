using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using YTMediaControllerSrv.Controller;
using YTMediaControllerSrv.Types;


namespace YTMediaControllerSrv.Server
{
    internal class ControlServer
    {
        public WebSocketConnectionManager wsManager;

        public ControlServer(string host, int port)
        {
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
            }
        }
    }
}