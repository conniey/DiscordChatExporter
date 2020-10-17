using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot.Work
{
    public class RemoveRoleWork : IWork
    {
        private readonly ISocketMessageChannel _executedChannel;
        private readonly SocketGuild _guild;
        private readonly ILogger<RemoveRoleWork> _logger;
        private readonly string _roleName;
        private readonly string _description;
        private readonly Func<SocketGuildUser, bool> _removalPredicate;

        public RemoveRoleWork(string requestId, ISocketMessageChannel executedChannel, SocketGuild guild,
            string roleName, string description, Func<SocketGuildUser, bool> removalPredicate,
            ILogger<RemoveRoleWork> logger)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException($"'{nameof(roleName)}' cannot be null or empty", nameof(roleName));
            }

            _executedChannel = executedChannel ?? throw new ArgumentNullException(nameof(executedChannel));
            _guild = guild;
            _roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
            _description = description;
            _removalPredicate = removalPredicate;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            RequestId = requestId ?? throw new ArgumentNullException(nameof(requestId));
        }

        public string RequestId { get; }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started removing '{Role}' for users.", _roleName);
            await _guild.DownloadUsersAsync().ConfigureAwait(false);

            SocketRole socketRole = _guild.Roles.Where(x => x.Name.Equals(_roleName)).SingleOrDefault();

            if (socketRole == default)
            {
                _logger.LogWarning("Could not locate '{Role}' role.", _roleName);
                return;
            }

            var usersToRemove = socketRole.Members.Where(x => _removalPredicate(x)).OrderBy(e => e.JoinedAt).ToList();
            var failedToRemove = new List<SocketGuildUser>();
            var successful = new List<SocketGuildUser>();
            var stringBuilder = new StringBuilder();

            if (usersToRemove.Any())
            {
                stringBuilder.AppendLine($"Joined at\t\tUser");
            }
            else
            {
                stringBuilder.AppendLine("There were no matching users to remove.");
            }

            for (int i = 0; i < usersToRemove.Count; i++)
            {
                SocketGuildUser user = usersToRemove[i];
                try
                {
                    _logger.LogInformation("Removing: {JoinedAt}\t@{User}:{Discriminator}",
                        user.Username, user.Discriminator, user.JoinedAt);
                    stringBuilder.AppendLine($"{user.JoinedAt:r}\t<@{user.Id}>");

                    await user.RemoveRoleAsync(socketRole).ConfigureAwait(false);
                    successful.Add(user);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to remove {User}. Id: {Id}", user.Username, user.Id);
                    failedToRemove.Add(user);
                }
            }

            var embedBuilder = new EmbedBuilder().WithDescription(stringBuilder.ToString());

            if (failedToRemove.Any())
            {
                embedBuilder.AddField("Failed", string.Join(", ", failedToRemove.Select(x => x.Id)));
            }

            embedBuilder
                .AddField("Total", usersToRemove.Count, inline: true)
                .AddField("Success", successful.Count, inline: true)
                .AddField("Failed", failedToRemove.Count, inline: true);

            await _executedChannel.SendMessageAsync(_description, embed: embedBuilder.Build())
                .ConfigureAwait(false);
        }
    }
}
