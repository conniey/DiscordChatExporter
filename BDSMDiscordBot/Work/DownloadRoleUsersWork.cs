using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordImporterBot.Work
{
    public class DownloadRoleUsersWork : IWork
    {
        private readonly ISocketMessageChannel _executedInChannel;
        private readonly SocketGuild _originalGuild;
        private readonly SocketTextChannel _destinationTextChannel;
        private readonly HashSet<string> _matchedRoleNames;
        private readonly ILogger<DownloadRoleUsersWork> _logger;
        private readonly string _roleNamesString;

        public DownloadRoleUsersWork(ISocketMessageChannel executedInChannel,
            SocketGuild originalGuild,
            SocketTextChannel destinationTextChannel,
            HashSet<string> matchedRoleNames,
            string requestId,
            ILogger<DownloadRoleUsersWork> logger)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException($"'{nameof(requestId)}' cannot be null or empty", nameof(requestId));
            }

            RequestId = requestId;
            _executedInChannel = executedInChannel ?? throw new ArgumentNullException(nameof(executedInChannel));
            _originalGuild = originalGuild ?? throw new ArgumentNullException(nameof(originalGuild));
            _destinationTextChannel = destinationTextChannel ?? throw new ArgumentNullException(nameof(destinationTextChannel));
            _matchedRoleNames = matchedRoleNames ?? throw new ArgumentNullException(nameof(matchedRoleNames));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _roleNamesString = string.Join(", ", _matchedRoleNames);
        }

        public string RequestId { get; }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started exporting users.");
            await _originalGuild.DownloadUsersAsync().ConfigureAwait(false);

            // Add the first set of members from the first role.
            var roles = _originalGuild.Roles.Where(x => _matchedRoleNames.Contains(x.Name));
            var matchingMembers = new HashSet<SocketGuildUser>(roles.First().Members, SocketGuildUserComparator.Instance);

            foreach (var r in roles.Skip(1))
            {
                matchingMembers.IntersectWith(r.Members);
            }

            using (Stream stream = new MemoryStream())
            {
                _logger.LogInformation("Writing users...");
                int numberOfUsers = 0;
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                {
                    await writer.WriteLineAsync("```").ConfigureAwait(false);
                    await writer.WriteLineAsync("Member Id\tUsername");
                    foreach (var member in matchingMembers)
                    {
                        numberOfUsers++;
                        await writer.WriteLineAsync($"{member.Id}\t{member.Username}#{member.Discriminator}")
                            .ConfigureAwait(false);
                    }

                    await writer.WriteLineAsync("```").ConfigureAwait(false);
                    await writer.FlushAsync().ConfigureAwait(false);
                }

                // Reset the stream to the beginning.
                stream.Seek(0, SeekOrigin.Begin);

                var embed = new EmbedBuilder()
                    .WithCurrentTimestamp()
                    .AddField("# Users", numberOfUsers, true)
                    .AddField("Roles", _roleNamesString)
                    .Build();

                _logger.LogInformation("Uploading file....");
                await _destinationTextChannel.SendFileAsync(stream, "users.txt", "Exported users", embed: embed)
                    .ConfigureAwait(false);

                _logger.LogInformation("Finished exporting users. Roles '{Roles}'", _matchedRoleNames);
                await _executedInChannel.SendMessageAsync($"Finished exporting. Roles '{_roleNamesString}'")
                    .ConfigureAwait(false);
            }
        }
    }
}
