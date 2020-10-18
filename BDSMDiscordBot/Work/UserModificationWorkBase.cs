using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BDSMDiscordBot.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot.Work
{
    public abstract class UserModificationWorkBase : IWork
    {
        private readonly string _description;
        private readonly SocketGuild _guild;
        private readonly ISocketMessageChannel _executedChannel;
        private readonly ILogger<UserModificationWorkBase> _logger;

        public UserModificationWorkBase(string requestId, ISocketMessageChannel executedChannel,
            SocketGuild guild, string description, ILogger<UserModificationWorkBase> logger)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentException($"'{nameof(requestId)}' cannot be null or empty", nameof(requestId));
            }
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException($"'{nameof(description)}' cannot be null or empty", nameof(description));
            }

            RequestId = requestId;
            _guild = guild ?? throw new ArgumentNullException(nameof(guild));
            _description = description;
            _executedChannel = executedChannel ?? throw new ArgumentNullException(nameof(executedChannel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string RequestId { get; }

        /// <summary>
        /// Gets a set of users to modify.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns>null if there was a problem getting mathcing users. Otherwise, a list.</returns>
        protected abstract Task<List<SocketGuildUser>> GetMatchingUsers(SocketGuild guild);

        protected abstract Task ModifyUserAsync(SocketGuildUser user);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started '{RequestId}': {Description}.", RequestId, _description);

            var usersToRemove = await GetMatchingUsers(_guild).ConfigureAwait(false);
            if (usersToRemove == default)
            {
                _logger.LogInformation("There was a problem getting matching users.");
                return;
            }

            var failed = new List<SocketGuildUser>();
            var successful = new List<SocketGuildUser>();
            var stringBuilder = new StringBuilder();

            if (usersToRemove.Any())
            {
                stringBuilder.AppendLine($"Joined at\t\tUser");
            }
            else
            {
                stringBuilder.AppendLine("There were no matching users.");
            }

            for (int i = 0; i < usersToRemove.Count; i++)
            {
                SocketGuildUser user = usersToRemove[i];
                try
                {
                    _logger.LogInformation("Modifying: {JoinedAt}\t@{User}", user.JoinedAt, user.ToDisplayName());
                    stringBuilder.AppendLine($"{user.JoinedAt:u}\t<@{user.Id}>");

                    await ModifyUserAsync(user).ConfigureAwait(false);
                    successful.Add(user);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to modify {User}. Id: {Id}", user.ToDisplayName(), user.Id);
                    failed.Add(user);
                }
            }

            var embedBuilder = new EmbedBuilder().WithDescription(stringBuilder.ToString());

            if (failed.Any())
            {
                embedBuilder.AddField("Failed", string.Join(", ", failed.Select(x => x.Id)));
            }

            embedBuilder
                .AddField("Total", usersToRemove.Count, inline: true)
                .AddField("Success", successful.Count, inline: true)
                .AddField("Failed", failed.Count, inline: true);

            await _executedChannel.SendMessageAsync(_description, embed: embedBuilder.Build())
                .ConfigureAwait(false);
        }
    }
}
