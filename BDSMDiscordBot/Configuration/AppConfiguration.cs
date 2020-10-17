using System.Collections.Generic;

namespace BDSMDiscordBot.Configuration
{
    public class AppConfiguration
    {
        public string Prefix { get; set; }

        public string RootDirectory { get; set; }

        public GuildConfiguration Guild { get; set; }

        public AllowedUsersConfiguration AllowedUsers { get; set; }

        public List<string> DisabledModules { get; set; }
    }

    public class GuildConfiguration
    {
        /// <summary>
        /// Original server id.
        /// </summary>
        public string Original { get; set; }

        /// <summary>
        /// Back up server id.
        /// </summary>
        public string New { get; set; }
    }

    /// <summary>
    /// Users allowed to execute commands.
    /// </summary>
    public class AllowedUsersConfiguration
    {
        /// <summary>
        /// List of user ids.
        /// </summary>
        public List<string> Users { get; set; }

        /// <summary>
        /// List of role names or role ids.
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
