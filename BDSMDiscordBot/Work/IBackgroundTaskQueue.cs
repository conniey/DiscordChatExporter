using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordImporterBot.Work
{
    /// <summary>
    /// https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services#queued-background-tasks
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        public void QueueBackgroundWorkItem(IWork workItem);

        public Task<IWork> DequeueAsync(CancellationToken cancellationToken);
    }
}
