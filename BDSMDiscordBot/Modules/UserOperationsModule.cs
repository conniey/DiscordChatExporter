using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using DiscordImporterBot.Configuration;
using DiscordImporterBot.Work;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImporterBot
{
    [Group("users")]
    public class UserOperationsModule : ModuleBase<SocketCommandContext>
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly PermissionResolver _permissionResolver;
        private readonly ILogger<UserOperationsModule> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public UserOperationsModule(IBackgroundTaskQueue taskQueue, PermissionResolver permissionResolver,
            ILogger<UserOperationsModule> logger, ILoggerFactory loggerFactory)
        {
            _taskQueue = taskQueue;
            _permissionResolver = permissionResolver;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        [Command("export")]
        [Summary("Exports users who match a particular set of roles.")]
        public async Task ExportUsersAsync(
            [Summary("Destination Guild Id")]ulong destinationGuildId,
            [Summary("Destination Channel Id")]ulong destinationChannelId,
            [Summary("Roles")] params string[] roles)
        {
            if (!_permissionResolver.HasPermission(Context.Guild, Context.User))
            {
                _logger.LogWarning("{User} does not have permission to export.", Context.User.Username);
                return;
            }

            _logger.LogInformation("Exporting users with roles [{Roles}]", roles);

            if (roles.Length == 0)
            {
                await Context.Channel.SendMessageAsync("There are no roles specified. Nothing to export.").ConfigureAwait(false);
                return;
            }

            var channel = Context.Client.GetGuild(destinationGuildId).GetTextChannel(destinationChannelId);
            if (channel == null)
            {
                _logger.LogWarning("{GuildId}: Unable to get {Channel} to upload message to.",
                    destinationGuildId, destinationChannelId);
                return;
            }

            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("{RequestId}: Queuing work.", requestId);

            var roleSet = new HashSet<string>(roles);
            var work = new DownloadRoleUsersWork(Context.Channel, Context.Guild, channel, roleSet, requestId,
                _loggerFactory.CreateLogger<DownloadRoleUsersWork>());

            _taskQueue.QueueBackgroundWorkItem(work);

            _logger.LogInformation("{RequestId}: Queued.", requestId);
        }

        [Command("clear-noobs")]
        [Summary("Removes a role from users who have had the role longer than 2 weeks.")]
        public async Task RemoveRoleAsync(
            [Summary("Role to remove")]string role = ".")
        {
            if (!_permissionResolver.HasPermission(Context.Guild, Context.User))
            {
                _logger.LogWarning("{User} does not have permission to remove roles.", Context.User.Username);
                return;
            }

            DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddDays(-14);
            bool isOlderThanTwoWeeks(SocketGuildUser x)
            {
                // -1 means that the joinDate is earlier than the cut-off date.
                return !x.IsBot && x.JoinedAt.HasValue && x.JoinedAt.Value.CompareTo(dateTimeOffset) == -1;
            };


            if (string.IsNullOrEmpty(role))
            {
                await Context.Channel.SendMessageAsync("'role' cannot be a null or empty string.")
                    .ConfigureAwait(false);
                return;
            }

            var work = new RemoveRoleWork($"clear-{role}", Context.Channel, Context.Guild, role,
                $"Removing '{role}' from users older than {dateTimeOffset:R}.", isOlderThanTwoWeeks,
                _loggerFactory.CreateLogger<RemoveRoleWork>());

            _taskQueue.QueueBackgroundWorkItem(work);
            _logger.LogInformation("{RequestId}: Queued.", work.RequestId);
        }
    }
}
