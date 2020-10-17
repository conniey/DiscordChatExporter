using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BDSMDiscordBot.Models;
using BDSMDiscordBot.Work;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot
{
    /// <summary>
    /// Functions dealing with users.
    /// </summary>
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

        /// <summary>
        /// Exports users who have a certain role from the executing server to the next one.
        /// </summary>
        /// <param name="destinationGuildId">Destination Discord server.</param>
        /// <param name="destinationChannelId">Destination channel.</param>
        /// <param name="roles">Roles to match. This is the intersection of all the roles there.</param>
        /// <returns>Task when it completes.</returns>
        [Command("export")]
        [Summary("Exports users who match a particular set of roles.")]
        public async Task ExportUsersAsync(
            [Summary("Destination Guild Id")] ulong destinationGuildId,
            [Summary("Destination Channel Id")] ulong destinationChannelId,
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

        /// <summary>
        /// Removes a role from useres who have the matching role. By default it removes the "." (ie. newbie) role.
        /// </summary>
        /// <param name="role">The role to remove.</param>
        /// <returns>Task when it completes.</returns>
        [Command("clear")]
        [Summary("Removes a role from users who have had the role longer than 2 weeks.")]
        public async Task RemoveRoleAsync(
            [Summary("Role to remove")] string role = ".")
        {
            if (!_permissionResolver.HasPermission(Context.Guild, Context.User))
            {
                _logger.LogWarning("{User} does not have permission to remove roles.", Context.User.Username);
                return;
            }

            if (string.IsNullOrEmpty(role))
            {
                await Context.Channel.SendMessageAsync("'role' cannot be a null or empty string.")
                    .ConfigureAwait(false);
                return;
            }

            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-14);
            var work = new RemoveRoleWork($"clear-{role}", Context.Channel, Context.Guild, role,
                $"Removing '{role}' from users older than {cutoffDate:R}.",
                user => user.IsJoinDateOlderThan(cutoffDate),
                _loggerFactory.CreateLogger<RemoveRoleWork>());

            _taskQueue.QueueBackgroundWorkItem(work);
            _logger.LogInformation("{RequestId}: Queued.", work.RequestId);
        }
    }
}
