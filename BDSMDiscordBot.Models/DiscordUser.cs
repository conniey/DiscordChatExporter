namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/user#user-object
    public class DiscordUser : Identifiable
    {
        public string? Id { get; set; }

        public bool IsBot { get; set; }

        /// <summary>
        /// Gets the integer value of the discriminator.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return 143.
        /// </example>
        public int? Discriminator { get; set; }

        /// <summary>
        /// Gets the username of the user.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return Foo.
        /// </example>
        public string? Username { get; set; }

        /// <summary>
        /// Gets the username with the discriminator.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return Foo#0143.
        /// </example>
        public string FullName => $"{Username}#{Discriminator:0000}";

        /// <summary>
        /// Gets the URL for the profile picture.
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}
