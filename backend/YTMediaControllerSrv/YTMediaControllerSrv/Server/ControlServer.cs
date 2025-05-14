using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using YTMediaControllerSrv.Controller;
using YTMediaControllerSrv.Types;


namespace YTMediaControllerSrv.Server
{
    internal class ControlServer
    {
        public NamedPipeServerApi pipeApi = new NamedPipeServerApi("YTMediaControllerPipe");

        public ControlServer()
        {
            pipeApi.OnMessageReceived += OnMessage;
            pipeApi.OnClientConnected += OnConnected;
            pipeApi.OnClientDisconnected += OnDisconnected;
        }
        public void Start()
        {
           pipeApi.Start();
        }

        public void Stop()
        {
            pipeApi.Dispose();
        }

        private void OnConnected()
        {
            Console.WriteLine("Client connected to the named pipe server.");
        }

        private void OnDisconnected()
        {
            Console.WriteLine("Client disconnected from the named pipe server.");
        }

        public async Task Send(object jsonObject)
        {
            if (pipeApi.IsClientConnected())
            {
               await pipeApi.Send(jsonObject);
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