using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BDSMDiscordBot.Configuration;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BDSMDiscordBot
{
    public class ContentResolver
    {
        private const string DISCORD_APP_URL = "discordapp.com";

        private readonly DiscordSocketClient _client;
        private readonly ILogger<ContentResolver> _logger;
        private readonly string _contentRoot;
        private readonly ulong _guildId;

        public ContentResolver(DiscordSocketClient client, IOptions<AppConfiguration> options,
            ILogger<ContentResolver> logger)
        {
            _client = client;
            _logger = logger;
            _contentRoot = options.Value.RootDirectory;
            _guildId = ulong.Parse(options.Value.Guild.Original);
        }

        public string GetLocalPath(string directory, string filename)
        {
            return Path.Combine(_contentRoot, directory, filename);
        }

        /// <summary>
        /// Given the following URL, tries to fetch the message contents.
        /// Only works in current guild.
        /// https://discordapp.com/channels/320009916863479808/755402562907406417/759868716086460481
        /// </summary>
        /// <param name="url">URL to fetch.</param>
        /// <returns>null if it was not the correct guild or url is not a message link.</returns>
        public async Task<IMessage> GetMessageAsync(string url)
        {
            var uri = new Uri(url);

            if (!IsDiscordLink(uri))
            {
                _logger.LogWarning($"{url} is not a discord message link. Will not be resolved.");
                return null;
            }

            var parsed = uri.Segments.Select(x => x.Replace("/", string.Empty)).ToArray();
            if (parsed.Length != 5)
            {
                _logger.LogWarning($"{url} does not have 5 segments.");
                return null;
            }

            if (!string.Equals("channels", parsed[1], StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("{Url} does not contain the channel segment. {Segment}.", url, parsed[1]);
                return null;
            }
            if (!ulong.TryParse(parsed[2], out var guildId))
            {
                _logger.LogWarning("{Url} does not contain a valid guildId segment. {Segment}.", url, parsed[2]);
                return null;
            }
            if (_guildId != guildId)
            {
                _logger.LogWarning("{Url} does not contain the appropriate guildId. {Expected} != {Actual}", url,
                    _guildId, guildId);
                return null;
            }
            if (!ulong.TryParse(parsed[3], out var channelId))
            {
                _logger.LogWarning("{Url} does not contain a valid channelId segment. {Segment}.", url, parsed[3]);
                return null;
            }
            if (!ulong.TryParse(parsed[4], out var messageId))
            {
                _logger.LogWarning("{Url} does not contain a valid messageId segment. {Segment}.", url, parsed[4]);
                return null;
            }

            var textChannel = _client.GetGuild(_guildId).GetTextChannel(channelId);
            var message = await textChannel.GetMessageAsync(messageId).ConfigureAwait(false);
            return message;
        }

        public static bool IsDiscordLink(string url)
        {
            var uri = new Uri(url);
            return IsDiscordLink(uri);
        }

        private static bool IsDiscordLink(Uri uri)
        {
            return DISCORD_APP_URL == uri.Host;
        }
    }
}
