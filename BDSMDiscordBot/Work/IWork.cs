using System.Threading;
using System.Threading.Tasks;

namespace DiscordImporterBot.Work
{
    /// <summary>
    /// Represents some long running work.
    /// </summary>
    public interface IWork
    {
        string RequestId { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
