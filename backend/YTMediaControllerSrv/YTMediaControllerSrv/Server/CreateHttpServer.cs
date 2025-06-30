using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YTMediaControllerSrv.Server
{
    internal class CreateHttpServer
    {
        private HttpListener listener = new HttpListener();
        public delegate void OnRequestHandler(HttpListenerContext context, HttpListenerRequest request, HttpListenerResponse response);
        public event OnRequestHandler OnRequest;

        public CreateHttpServer(string url)
        {
            listener.Prefixes.Add(url);
            listener.Start();
            Task.Run(() => HandleRequests(listener));
        }

        public void Stop()
        {
            listener.Stop();
        }

        async private void HandleRequests(HttpListener listener)
        {
            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    OnRequest(context, request, response);
                }
                catch (Exception e)
                {
                    Logger.Error("Unable to start HTTP server", e);
                }
            }
        }
    }
}
