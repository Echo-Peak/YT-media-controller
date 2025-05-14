using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YTMediaControllerSrv.Server
{
    internal class NamedPipeServerApi
    {
        private readonly string _pipeName;
        private NamedPipeServerStream _pipeServer;
        private CancellationTokenSource _cts;
        private StreamWriter _writer;
        private Task _listenTask;

        public event Action<string> OnMessageReceived;
        public event Action OnClientConnected;
        public event Action OnClientDisconnected;

        public NamedPipeServerApi(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start(CancellationToken externalToken = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _listenTask = Task.Run(() => ListenForConnectionsAsync(_cts.Token), _cts.Token);
        }

        private async Task ListenForConnectionsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _pipeServer = new NamedPipeServerStream(
                        _pipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    Console.WriteLine("Waiting for connection...");
                    await _pipeServer.WaitForConnectionAsync(token);

                    OnClientConnected?.Invoke();
                    await HandleClientCommunicationAsync(token);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[NamedPipeServerApi] Error accepting client connection: {ex.Message}");
                }
            }
        }

        private async Task HandleClientCommunicationAsync(CancellationToken token)
        {
            try
            {
                using (var reader = new StreamReader(_pipeServer, Encoding.UTF8))
                {
                    _writer = new StreamWriter(_pipeServer, Encoding.UTF8) { AutoFlush = true };

                        while (IsClientConnected() && !token.IsCancellationRequested)
                        {
                            string message = await reader.ReadLineAsync();
                            if (message == null) break;

                            Console.WriteLine("Received from client: " + message);
                            OnMessageReceived?.Invoke(message);

                        }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NamedPipeServerApi] Error during communication: {ex.Message}");
            }
            finally
            {
                _writer = null;
                OnClientDisconnected?.Invoke();
                _pipeServer?.Dispose();
            }
        }

        public async Task Send(object jsonObject)
        {
            if (!IsClientConnected() || _writer == null)
            {
                Console.Error.WriteLine("[NamedPipeServerApi] Cannot send: No client is connected.");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(jsonObject);
                await _writer.WriteLineAsync(json);
                Console.WriteLine("[NamedPipeServerApi] Sent to client: " + json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NamedPipeServerApi] Send error: {ex.Message}");
            }

        }

        public bool IsClientConnected()
        {
            return _pipeServer?.IsConnected == true;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _listenTask?.Wait();
            _pipeServer?.Dispose();
            _cts?.Dispose();
        }
    }
}
