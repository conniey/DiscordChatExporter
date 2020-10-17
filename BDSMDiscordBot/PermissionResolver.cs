using System;
using System.Collections.Generic;
using System.Linq;
using BDSMDiscordBot.Configuration;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace BDSMDiscordBot
{
    /// <summary>
    /// Resolves permissions and whether a user can use a command.
    /// </summary>
    public class PermissionResolver
    {
        private readonly HashSet<ulong> _allowedUsers;
        private readonly HashSet<string> _roles;

        public PermissionResolver(DiscordSocketClient client, IOptions<AppConfiguration> options)
        {
            var configuration = options.Value ?? throw new ArgumentNullException("'appConfiguration' cannot be null.");
            var allowedUsersConfiguration = configuration.AllowedUsers;

            _allowedUsers = new HashSet<ulong>(allowedUsersConfiguration?.Users?.Select(x => ulong.Parse(x))
                ?? Enumerable.Empty<ulong>());
            _roles = new HashSet<string>(allowedUsersConfiguration.Roles);
        }

        /// <summary>
        /// Gets whether or not <paramref name="user"/> can invoke operations on the bot or not.
        /// </summary>
        /// <param name="guild">Guild to get roles from.</param>
        /// <param name="user">User to match.</param>
        /// <returns>true if the user has permission; false otherwise.</returns>
        public bool HasPermission(SocketGuild guild, SocketUser user)
        {
            if (guild == null)
            {
                throw new ArgumentNullException(nameof(guild));
            }
            else if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (_allowedUsers.Contains(user.Id))
            {
                return true;
            }

            return guild.Roles
                .Where(role => _roles.Contains(role.Id.ToString()) || _roles.Contains(role.Name))
                .Any(role => role.Members.Any(member => user.Id == member.Id));
        }
    }
}
