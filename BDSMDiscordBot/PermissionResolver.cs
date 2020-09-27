using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordImporterBot.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImporterBot
{
    public class PermissionResolver
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<PermissionResolver> _logger;
        private readonly ulong _originalGuildId;
        private readonly HashSet<ulong> _allowedUsers;
        private readonly AllowedUsersConfiguration _allowedUsersConfiguration;
        private volatile int isInitialized = 0;

        public PermissionResolver(DiscordSocketClient client, IOptions<AppConfiguration> options, ILogger<PermissionResolver> logger)
        {
            _client = client;
            _logger = logger;

            var configuration = options.Value ?? throw new ArgumentNullException("'appConfiguration' cannot be null.");
            _originalGuildId = ulong.Parse(configuration.Guild.Original);
            _allowedUsersConfiguration = configuration.AllowedUsers;
            _allowedUsers = new HashSet<ulong>(_allowedUsersConfiguration?.Users?.Select(x => ulong.Parse(x))
                ?? Enumerable.Empty<ulong>());
        }

        public bool HasPermission(SocketGuild guild, SocketUser user)
        {
            if (Interlocked.CompareExchange(ref isInitialized, 1, 0) == 0)
            {
                var roles = new HashSet<string>(_allowedUsersConfiguration.Roles);

                var members = guild.Roles
                    .Where(role => roles.Contains(role.Id.ToString()) || roles.Contains(role.Name))
                    .SelectMany(role => role.Members);

                if (!members.Any())
                {
                    _logger.LogWarning("Could not find any users in roles: {Roles}", _allowedUsersConfiguration.Roles);
                }

                foreach (var member in members)
                {
                    if (!_allowedUsers.Contains(member.Id))
                    {
                        _allowedUsers.Add(member.Id);
                    }
                }
            }

            return _allowedUsers.Contains(user.Id);
        }
    }
}
