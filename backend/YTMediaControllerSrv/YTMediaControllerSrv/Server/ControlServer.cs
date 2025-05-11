using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv.Controller;


namespace YTMediaControllerSrv.Server
{
    internal class ControlServer
    {
        public CreateWebSockerServer server { get; set; }
        public ControlServer(string host, int port)
        {
            server = new CreateWebSockerServer(host, port);
            server.OnMessage += OnMessage;

            Task.Run(() => server.Start());
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Send(object data)
        {
            foreach (var client in server.Clients)
            {
                if (client.Key.State == WebSocketState.Open)
                {
                    string json = JsonConvert.SerializeObject(data);
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    client.Key.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        public void OnMessage(WebSocket ws, string action)
        {
            switch (action)
            {
                case "playbackStated":
                    {
                        SystemController.EnterFullScreen();
                        break;
                    }
            }
        }
    }
}