using System.Threading;
using System.Threading.Tasks;

namespace BDSMDiscordBot.Work
{
    /// <summary>
    /// Represents some long running work.
    /// </summary>
    public interface IWork
    {
        /// <summary>
        /// Request ID for the work.
        /// </summary>
        string RequestId { get; }

        /// <summary>
        /// Executes the work and returns a task when it completes.
        /// </summary>
        /// <param name="cancellationToken">Token when it completes.</param>
        /// <returns>A Task that completes when it finishes.</returns>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
