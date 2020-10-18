using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDSMDiscordBot.Models;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot.Work
{
    /// <summary>
    /// Work that removes a role from matching users.
    /// </summary>
    public class RemoveRoleWork : UserModificationWorkBase
    {
        private readonly ILogger<RemoveRoleWork> _logger;
        private readonly DateTimeOffset _cutoffDate;
        private readonly string _roleToRemove;
        private readonly Lazy<Task<SocketRole>> _socketRole;

        public RemoveRoleWork(string requestId, ISocketMessageChannel executedChannel, SocketGuild guild,
            string description, ILogger<RemoveRoleWork> logger, DateTimeOffset cutoffDate, string roleToRemove)
            : base(requestId, executedChannel, guild, description, logger)
        {
            _logger = logger;
            _cutoffDate = cutoffDate;
            _roleToRemove = roleToRemove;
            _socketRole = new Lazy<Task<SocketRole>>(async () =>
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                return guild.Roles.Where(x => x.Name.Equals(roleToRemove)).SingleOrDefault();
            });
        }

        protected override async Task<List<SocketGuildUser>> GetMatchingUsers(SocketGuild guild)
        {
            var role = await _socketRole.Value.ConfigureAwait(false);
            if (role == default)
            {
                return null;
            }

            return role.Members.Where(user => user.IsJoinDateOlderThan(_cutoffDate))
                .OrderBy(e => e.JoinedAt)
                .ToList();
        }

        protected override async Task ModifyUserAsync(SocketGuildUser user)
        {
            var role = await _socketRole.Value.ConfigureAwait(false);
            if (role == default)
            {
                _logger.LogWarning("Unable to modify user because '{Role}' does not exist.", _roleToRemove);
                return;
            }

            await user.RemoveRoleAsync(role).ConfigureAwait(false);
        }
    }
}
