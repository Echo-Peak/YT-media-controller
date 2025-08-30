using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YTMediaControllerSrv.Logging;

namespace YTMediaControllerSrv.Server
{
    internal class WebSocketConnectionManager
    {
        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<string> OnMessage;
        private string wsUrl;
        private WebSocket _clientSocket;
        private readonly ILogger Logger;

        public WebSocketConnectionManager(string urlPrefix, ILogger logger)
        {
            Logger = logger;
            wsUrl = urlPrefix;
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(urlPrefix);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _httpListener.Start();
            Logger.Info($"[WebSocketServer] Listening at {wsUrl}...");


            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _httpListener.GetContextAsync();

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    continue;
                }

                if (_clientSocket != null && _clientSocket.State == WebSocketState.Open)
                {
                    context.Response.StatusCode = 409;
                    context.Response.Close();
                    Logger.Info("[WebSocketServer] Rejected connection: already connected.");
                    continue;
                }

                var wsContext = await context.AcceptWebSocketAsync(null);
                _clientSocket = wsContext.WebSocket;

                OnConnect?.Invoke();
                Logger.Info("[WebSocketServer] Client connected");

                _ = ListenAsync(_clientSocket, cancellationToken);
            }
        }

        private async Task ListenAsync(WebSocket socket, CancellationToken ct)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Logger.Debug(message);
                    OnMessage?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[WebSocketServer] error", ex);
            }
            finally
            {
                OnDisconnect?.Invoke();
                Logger.Info("[WebSocketServer] Client disconnected");

                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);

                _clientSocket = null;
            }
        }

        public async Task SendAsync(object data)
        {
            Logger.Info("Sending data to UI web sockets");
            string message = JsonConvert.SerializeObject(data);
            if (_clientSocket?.State == WebSocketState.Open)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _clientSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public bool IsConnected()
        {
            return _clientSocket != null && _clientSocket.State == WebSocketState.Open;
        }

        public void Stop()
        {
            _httpListener.Stop();
        }
    }
}
