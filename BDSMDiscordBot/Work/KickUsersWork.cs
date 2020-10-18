using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDSMDiscordBot.Models;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace BDSMDiscordBot.Work
{
    public class KickUsersWork : UserModificationWorkBase
    {
        private readonly SocketGuild _guild;
        private readonly ILogger<KickUsersWork> _logger;
        private readonly DateTimeOffset _cutoffDate;
        private readonly string _verifiedRole;

        public KickUsersWork(string requestId, ISocketMessageChannel executedChannel, SocketGuild guild,
            string description, ILogger<KickUsersWork> logger, DateTimeOffset cutoffDate, string verifiedRole)
            : base(requestId, executedChannel, guild, description, logger)
        {
            _guild = guild;
            _logger = logger;
            _cutoffDate = cutoffDate;
            _verifiedRole = verifiedRole;
        }

        protected override async Task<List<SocketGuildUser>> GetMatchingUsers(SocketGuild guild)
        {
            await guild.DownloadUsersAsync().ConfigureAwait(false);

            var socketRole = _guild.Roles.Where(x => x.Name.Equals(_verifiedRole)).SingleOrDefault();
            if (socketRole == default)
            {
                _logger.LogWarning("Could not locate matching '{Role}' role.", _verifiedRole);
                return null;
            }

            var unverifiedUsers = _guild.Users.Except(socketRole.Members)
                .Where(x => x.IsJoinDateOlderThan(_cutoffDate))
                .ToList();

            return unverifiedUsers;
        }

        protected override Task ModifyUserAsync(SocketGuildUser user)
        {
            return user.KickAsync("We require users to verify within two weeks. Feel free to rejoin when you wish to verify.");
        }
    }
}
