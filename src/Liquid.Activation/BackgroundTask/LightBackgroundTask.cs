using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Liquid.Activation
{
    /// <summary>
    /// Abstract class based on IHostedService that run over OWIN and LighBackgroundTask 
    /// prepare and execute a back ground tasks signed's.
    /// </summary>
    public abstract class LightBackgroundTask : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        /// <summary>
        /// Start a background task async
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _executingTask = ExecuteAsync(_cancelToken.Token);

            if (_executingTask.IsCompleted)
                return _executingTask;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stop a background task async
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
                return;

            try
            {
                _cancelToken.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        /// <summary>
        /// Execute a background Task async
        /// </summary>
        /// <param name="stoppingToken">Cancellation Token</param>
        /// <returns></returns>
        protected virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await ProcessAsync();
                await Task.Delay(5000, stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        /// <summary>
        /// Process a brackground task async.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ProcessAsync() {  }
    }
}