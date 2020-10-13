namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// A discord server.
    /// <seealso cref="https://discord.com/developers/docs/resources/guild#guild-object"/>
    /// </summary>
    public class DiscordGuild : Identifiable
    {
        /// <summary>
        /// Gets or sets the guild id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the guild name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the guild icon.
        /// </summary>
        public string? IconUrl { get; set; }
    }
}
