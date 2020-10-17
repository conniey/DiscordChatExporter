using System;
using System.Threading;
using System.Threading.Tasks;
using BDSMDiscordBot.Work;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot
{
    public class MessageUploaderService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _workQueue;
        private readonly ILogger<MessageUploaderService> _logger;

        public MessageUploaderService(IBackgroundTaskQueue workQueue, ILogger<MessageUploaderService> logger)
        {
            _workQueue = workQueue ?? throw new ArgumentNullException(nameof(workQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(workQueue));
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service.");

            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down.");

            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await _workQueue.DequeueAsync(stoppingToken).ConfigureAwait(false);

                if (workItem == null)
                {
                    _logger.LogWarning("Dequeued work item is null.");
                    continue;
                }

                _logger.LogDebug("Running work {RequestId}...", workItem.RequestId);
                await workItem.ExecuteAsync(stoppingToken).ConfigureAwait(false);
            }
        }
    }
}
