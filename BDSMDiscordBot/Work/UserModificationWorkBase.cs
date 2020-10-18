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
        private static readonly string TOTAL = "Total";
        private static readonly string SUCCESS = "Success";
        private static readonly string FAILED = "Failed";

        private static readonly int CHARACTER_LIMIT = 2000;

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

            var usersToModify = await GetMatchingUsers(_guild).ConfigureAwait(false);
            if (usersToModify == default)
            {
                _logger.LogInformation("There was a problem getting matching users.");
                return;
            }

            var failed = new List<SocketGuildUser>();
            var successful = new List<SocketGuildUser>();

            if (usersToModify.Any())
            {
                var embed = new EmbedBuilder()
                    .WithDescription("There were no matching users.")
                    .AddField(TOTAL, usersToModify.Count, inline: true)
                    .AddField(SUCCESS, successful.Count, inline: true)
                    .AddField(FAILED, failed.Count, inline: true)
                    .Build();

                await _executedChannel.SendMessageAsync(_description, embed: embed).ConfigureAwait(false);
                return;
            }

            for (int i = 0; i < usersToModify.Count; i++)
            {
                SocketGuildUser user = usersToModify[i];
                try
                {
                    _logger.LogInformation("Modifying: {JoinedAt}\t@{User}", user.JoinedAt, user.ToDisplayName());

                    await ModifyUserAsync(user).ConfigureAwait(false);
                    successful.Add(user);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to modify {User}. Id: {Id}", user.ToDisplayName(), user.Id);
                    failed.Add(user);
                }
            }

            var allEmbeds = GetEmbeds(successful, true).Concat(GetEmbeds(failed, false)).ToList();
            for (int i = 0; i < allEmbeds.Count; i++)
            {
                var embed = allEmbeds[i];
                if (i == 0)
                {
                    await _executedChannel.SendMessageAsync(_description, embed: embed).ConfigureAwait(false);
                }
            }
        }

        static List<Embed> GetEmbeds(List<SocketGuildUser> users, bool isSuccess)
        {
            List<Embed> embeds = new List<Embed>();

            List<SocketGuildUser> onNext = users;
            while (onNext.Any())
            {
                Embed embed = GetEmbed(onNext, users.Count, isSuccess, out var remaining);
                embeds.Add(embed);

                onNext = remaining;
            }

            return embeds;
        }

        static Embed GetEmbed(List<SocketGuildUser> users, int total, bool isSuccess,
            out List<SocketGuildUser> remainingUsers)
        {
            var builder = new StringBuilder("Joined at\t\tUser");
            int number = 0;

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                var contents = $"{user.JoinedAt:u}\t<@{user.Id}>";

                if ((builder.Length + contents.Length) > CHARACTER_LIMIT)
                {
                    break;
                }

                builder.Append(contents);
                number = i;
            }

            var embedBuilder = new EmbedBuilder()
                .WithDescription(builder.ToString())
                .AddField(TOTAL, total, inline: true);

            if (isSuccess)
            {
                embedBuilder.AddField(SUCCESS, number, inline: true)
                    .AddField(FAILED, "n/a", inline: true);
            }
            else
            {
                embedBuilder.AddField(SUCCESS, "n/a", inline: true)
                    .AddField(FAILED, number, inline: true);
            }

            remainingUsers = number < users.Count ? users.Skip(number).ToList() : new List<SocketGuildUser>();
            return embedBuilder.Build();
        }
    }
}
