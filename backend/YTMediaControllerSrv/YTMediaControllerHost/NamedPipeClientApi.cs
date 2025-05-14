using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace YTMediaControllerHost
{
    internal class NamedPipeClientApi : IDisposable
    {
        private readonly string _pipeName;
        private NamedPipeClientStream _pipeClient;
        private StreamReader _reader;
        private StreamWriter _writer;
        private CancellationTokenSource _retryCts;
        private Task _retryLoopTask;

        public bool IsConnected => _pipeClient?.IsConnected == true;

        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;

        public NamedPipeClientApi(string pipeName)
        {
            _pipeName = pipeName;
        }

        public void Start(CancellationToken externalToken = default)
        {
            _retryCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            _retryLoopTask = Task.Run(() => RetryLoopAsync(_retryCts.Token), _retryCts.Token);
        }

        private async Task RetryLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!IsConnected)
                {
                    bool success = await ConnectAsync(token);
                    if (success)
                    {
                        _ = Task.Run(() => ListenAsync(token), token);
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(15), token);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        private async Task<bool> ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await _pipeClient.ConnectAsync(cancellationToken);

                _reader = new StreamReader(_pipeClient, Encoding.UTF8);
                _writer = new StreamWriter(_pipeClient, Encoding.UTF8) { AutoFlush = true };

                OnConnected?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NamedPipeClientApi] Connection failed: {ex.Message}");
                DisposePipe();
                return false;
            }
        }

        private async Task ListenAsync(CancellationToken token)
        {
            try
            {
                while (IsConnected && !token.IsCancellationRequested)
                {
                    string message = await _reader.ReadLineAsync();
                    if (message == null) break;

                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NamedPipeClientApi] Listen failed: {ex.Message}");
            }
            finally
            {
                OnDisconnected?.Invoke();
                DisposePipe();
            }
        }

        public async Task<bool> ConnectAsync(int timeoutMs = 2000, CancellationToken cancellationToken = default)
        {
            _pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

            using (var timeoutCts = new CancellationTokenSource(timeoutMs))
            {
               using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
                {
                    try
                    {
                        await _pipeClient.ConnectAsync(linkedCts.Token);
                        _reader = new StreamReader(_pipeClient, Encoding.UTF8);
                        _writer = new StreamWriter(_pipeClient, Encoding.UTF8) { AutoFlush = true };
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Pipe connection failed: {ex.Message}");
                        Dispose();
                        return false;
                    }
                }
            }
        }

        public async Task<bool> SendMessageAsync(string message)
        {
            if (!IsConnected) return false;
            try
            {
                await _writer.WriteLineAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to write message to pipe: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ReadMessageAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return null;
            try
            {
                return await _reader.ReadLineAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unable to read message from pipe: {ex.Message}");
                return null;
            }
        }

        private void DisposePipe()
        {
            try
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _pipeClient?.Dispose();
            }
            catch { }

            _writer = null;
            _reader = null;
            _pipeClient = null;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _pipeClient?.Dispose();
        }
    }
}
