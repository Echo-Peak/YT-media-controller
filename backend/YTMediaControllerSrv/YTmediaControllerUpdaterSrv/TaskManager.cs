using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTMediaControllerUpdaterSrv
{
    public sealed class TaskManager : IDisposable
    {
        private readonly Func<CancellationToken, Task> _job;
        private readonly TimeSpan _interval;
        private readonly bool _runImmediately;
        private readonly bool _fixedRate;
        private readonly Action<Exception> _onError;

        private CancellationTokenSource _cts;
        private Task _loopTask;

        public TaskManager(Func<CancellationToken, Task> job, TimeSpan interval, bool runImmediately = true, bool fixedRate = true, Action<Exception> onError = null)
        {
            _job = job;
            _interval = interval;
            _runImmediately = runImmediately;
            _fixedRate = fixedRate;
            _onError = onError;
        }

        public void Start()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;
            _cts.Cancel();
            try { if (_loopTask != null) await _loopTask.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            _cts.Dispose();
            _cts = null;
            _loopTask = null;
        }

        public async Task TriggerNowAsync()
        {
            var token = _cts?.Token ?? CancellationToken.None;
            await SafeRunAsync(token).ConfigureAwait(false);
        }

        private async Task LoopAsync(CancellationToken token)
        {
            if (_runImmediately) await SafeRunAsync(token).ConfigureAwait(false);

            if (_fixedRate)
            {
                var next = DateTimeOffset.UtcNow + _interval;
                while (!token.IsCancellationRequested)
                {
                    var delay = next - DateTimeOffset.UtcNow;
                    if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                    await Task.Delay(delay, token).ConfigureAwait(false);
                    await SafeRunAsync(token).ConfigureAwait(false);
                    next = next + _interval;
                }
            }
            else
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(_interval, token).ConfigureAwait(false);
                    await SafeRunAsync(token).ConfigureAwait(false);
                }
            }
        }

        private async Task SafeRunAsync(CancellationToken token)
        {
            try { await _job(token).ConfigureAwait(false); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex) { _onError?.Invoke(ex); }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
