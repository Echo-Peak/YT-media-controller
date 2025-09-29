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
    public readonly struct WSNamespace
    {
        public string Value { get; }
        private WSNamespace(string v) => Value = v;
        public static WSNamespace From(string value) => new WSNamespace(value);
        public static readonly WSNamespace ExternalViewer = new WSNamespace("/externalViewer");
        public static readonly WSNamespace ChromeBackend = new WSNamespace("/chromeBackend");
        public static implicit operator string(WSNamespace ns) => ns.Value;
    }
    internal class WebSocketConnectionManager
    {
        private readonly HttpListener _httpListener;
        private readonly ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();

        public event Action OnConnect;
        public event Action OnDisconnect;
        public event Action<string> OnMessage;

        public event Action<WSNamespace> OnConnectNs;
        public event Action<WSNamespace> OnDisconnectNs;
        public event Action<WSNamespace, string> OnMessageNs;

        private string wsUrl;
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

                var ns = WSNamespace.From(NormalizeNamespace(context.Request.Url.AbsolutePath));

                if (_clients.TryGetValue(ns, out var existing) && existing.State == WebSocketState.Open)
                {
                    context.Response.StatusCode = 409;
                    context.Response.Close();
                    Logger.Info($"[WebSocketServer] Rejected connection for '{ns}': already connected.");
                    continue;
                }

                var wsContext = await context.AcceptWebSocketAsync(null);
                var socket = wsContext.WebSocket;
                _clients[ns] = socket;

                OnConnect?.Invoke();
                OnConnectNs?.Invoke(ns);
                Logger.Info($"[WebSocketServer] Client connected [{ns}]");

                _ = ListenAsync(ns, socket, cancellationToken);
            }
        }

        private async Task ListenAsync(WSNamespace ns, WebSocket socket, CancellationToken ct)
        {
            var buffer = new byte[4096];

            try
            {
                while (socket.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Logger.Debug(message);
                    OnMessage?.Invoke(message);
                    OnMessageNs?.Invoke(ns, message);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[WebSocketServer] error [{ns}]", ex);
            }
            finally
            {
                OnDisconnect?.Invoke();
                OnDisconnectNs?.Invoke(ns);
                Logger.Info($"[WebSocketServer] Client disconnected [{ns}]");

                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);

                _clients.TryRemove(ns, out _);
            }
        }

        public async Task SendAsync(object data)
        {
            Logger.Info("Sending data to all UI web sockets");
            var message = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(message);
            var toSend = _clients.Values.Where(s => s.State == WebSocketState.Open).ToArray();
            foreach (var s in toSend)
                await s.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendAsync(string ns, object data)
        {
            var message = JsonConvert.SerializeObject(data);
            var bytes = Encoding.UTF8.GetBytes(message);
            if (_clients.TryGetValue(NormalizeNamespace(ns), out var s) && s.State == WebSocketState.Open)
                await s.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public bool IsConnected()
        {
            return _clients.Values.Any(s => s.State == WebSocketState.Open);
        }

        public bool IsConnected(string ns)
        {
            return _clients.TryGetValue(NormalizeNamespace(ns), out var s) && s.State == WebSocketState.Open;
        }

        public void Stop()
        {
            _httpListener.Stop();
        }

        private static string NormalizeNamespace(string path)
        {
            var p = (path ?? string.Empty).Trim();
            if (p.Length == 0 || p == "/") return "/";
            if (!p.StartsWith("/")) p = "/" + p;
            return p;
        }
    }
}
