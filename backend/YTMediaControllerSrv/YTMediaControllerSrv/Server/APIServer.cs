using System.Net.WebSockets;
using System.Threading.Tasks;



namespace YTMediaControllerSrv.Server
{
    internal class APIServer
    {
        public CreateWebSockerServer server { get; set; }
        public APIServer(string host, SettingsJSON settings)
        {
            server = new CreateWebSockerServer(host, settings.APIServerPort);
            server.OnMessage += OnMessage;

            Task.Run(() => server.Start());
        }

        public void Stop()
        {
            server.Stop();
        }

        public void OnMessage(WebSocket ws, string action)
        {
            switch (action)
            {
                case "enterFullScreen":
                    {
                        break;
                    }
            }
        }
    }
}