using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YTMediaControllerSrv.Types;
using Newtonsoft.Json;

namespace YTMediaControllerSrv.Server
{
    internal class CreateWebSockerServer
    {
        private HttpListener listener = new HttpListener();
        public readonly ConcurrentDictionary<WebSocket, object> Clients;
        public event Action<WebSocket> OnConnect;
        public event Action<WebSocket, string> OnMessage;

        public CreateWebSockerServer(string url)
        {
            Clients = new ConcurrentDictionary<WebSocket, object>();
            listener.Prefixes.Add(url);
        }

        public async Task Start()
        {
            listener.Start();
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    _ = Task.Run(async () =>
                    {
                        HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                        await HandleWebSocket(wsContext);
                    });
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
        }

        private async Task HandleWebSocket(HttpListenerWebSocketContext wsContext)
        {
            WebSocket webSocket = wsContext.WebSocket;
            Clients.TryAdd(webSocket, null);
            Console.WriteLine("Client connected");
            OnConnect?.Invoke(webSocket);

            await Task.Run(async () =>
            {
                byte[] buffer = new byte[1024 * 4];

                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            break; // Client requested closure
                        }

                        string received = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine("Received: " + received);

                        try
                        {
                            WebSocketMessage message = JsonConvert.DeserializeObject<WebSocketMessage>(received);
                            if (message.Action != null)
                            {
                                Console.WriteLine($"Received action: \"{message.Action}\"");
                                OnMessage?.Invoke(webSocket, message.Action);
                            }
                            else
                            {
                                throw new Exception("Invalid JSON format");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid JSON received: " + ex.Message);
                            byte[] errorResponse = Encoding.UTF8.GetBytes("Invalid JSON format");
                            await webSocket.SendAsync(new ArraySegment<byte>(errorResponse), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket error: {ex.Message}");
                }
                finally
                {
                    // Ensure client removal and clean closure
                    Clients.TryRemove(webSocket, out _);
                    if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseSent)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                    Console.WriteLine("Client disconnected");
                }
            });
        }
    }
}
