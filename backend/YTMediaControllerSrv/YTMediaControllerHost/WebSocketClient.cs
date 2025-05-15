using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTMediaControllerHost
{
    internal class WebSocketClient
    {
        private readonly Uri _serverUri;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(10);
        private Task _reconnectTask;
        private bool _isReconnecting = false;
        private readonly object _lock = new object();

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<string> OnMessage;


        public WebSocketClient(string host, int port)
        {
            _serverUri = new Uri($"ws://{host}:{port}");
        }

        public async Task ConnectAsync()
        {
            lock (_lock)
            {
                if (_socket != null && _socket.State == WebSocketState.Open)
                    return;

                _cts = new CancellationTokenSource();
                _socket = new ClientWebSocket();
            }

            try
            {
                await _socket.ConnectAsync(_serverUri, _cts.Token);
                OnConnect?.Invoke();
                Console.WriteLine("[WebSocketClient] Connected");

                _ = ListenAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WebSocketClient] Connect error: {ex.Message}");
                StartReconnectLoop();
            }
        }

        private void StartReconnectLoop()
        {
            if (_isReconnecting) return;

            _isReconnecting = true;

            _reconnectTask = Task.Run(async () =>
            {
                while (_socket == null || _socket.State != WebSocketState.Open)
                {
                    Console.WriteLine("[WebSocketClient] Attempting reconnect...");
                    try
                    {
                        await ConnectAsync();
                    }
                    catch
                    {
                        // Swallow exceptions, try again after delay
                    }

                    await Task.Delay(_retryInterval);
                }

                _isReconnecting = false;
            });
        }

        private async Task ListenAsync()
        {
            var buffer = new byte[4096];

            try
            {
                while (_socket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    OnMessage?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WebSocketClient] Error: {ex.Message}");
            }
            finally
            {
                if (_socket.State == WebSocketState.Open)
                    await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

                _socket.Dispose();
                _socket = null;

                OnDisconnect?.Invoke();
                Console.WriteLine("[WebSocketClient] Disconnected");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_socket?.State == WebSocketState.Open)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task DisconnectAsync()
        {
            _cts.Cancel();

            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnecting", CancellationToken.None);
            }

            _socket?.Dispose();
            _socket = null;
            Console.WriteLine("[WebSocketClient] Disconnected (manual)");
        }
    }
}
