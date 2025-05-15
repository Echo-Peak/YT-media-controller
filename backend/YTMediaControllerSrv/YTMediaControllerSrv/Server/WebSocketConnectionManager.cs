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

namespace YTMediaControllerSrv.Server
{
    internal class WebSocketConnectionManager
    {
        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<string> OnMessage;

        private WebSocket _clientSocket;

        public WebSocketConnectionManager(string urlPrefix)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(urlPrefix);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _httpListener.Start();
            Console.WriteLine("[WebSocketServer] Listening...");


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
                    Console.WriteLine("[WebSocketServer] Rejected connection: already connected.");
                    continue;
                }

                var wsContext = await context.AcceptWebSocketAsync(null);
                _clientSocket = wsContext.WebSocket;

                OnConnect?.Invoke();
                Console.WriteLine("[WebSocketServer] Client connected");

                _ = ListenAsync(_clientSocket, cancellationToken);
            }
        }

        private async Task ListenAsync(WebSocket socket, CancellationToken ct)
        {
            var buffer = new byte[4096];

            try
            {
                Console.WriteLine(socket.State == WebSocketState.Open && !ct.IsCancellationRequested);
                while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    Console.WriteLine(result.MessageType);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine(message);
                    OnMessage?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WebSocketServer] Error: {ex.Message}");
            }
            finally
            {
                OnDisconnect?.Invoke();
                Console.WriteLine("[WebSocketServer] Client disconnected");

                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);

                _clientSocket = null;
            }
        }

        public async Task SendAsync(object data)
        {
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
