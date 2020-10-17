using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot.Modules
{
    [Group("message")]
    public class MessageModule : ModuleBase<SocketCommandContext>
    {
        private readonly ContentResolver _contentResolver;
        private readonly ILogger<MessageModule> _logger;

        public MessageModule(ContentResolver contentResolver, ILogger<MessageModule> logger)
        {
            _contentResolver = contentResolver;
            _logger = logger;
        }

        /// <summary>
        /// Resolves and returns an embed with the given message.
        /// </summary>
        /// <example>
        /// https://discordapp.com/channels/320009916863479808/755402562907406417/759868716086460481 will fetch the
        /// message from a particular guild/channel/message.
        /// </example>
        /// <param name="url">Discord URL to fetch.</param>
        /// <returns></returns>
        [Command("fetch")]
        [Summary("Fetches the given message.")]
        public async Task FetchMessageAsync(
            [Summary("url")] string url)
        {
            _logger.LogInformation("Fetching: {Url}", url);
            var message = await _contentResolver.GetMessageAsync(url).ConfigureAwait(false);
            _logger.LogInformation("Fetched: {Message}", message);
        }
    }
}
