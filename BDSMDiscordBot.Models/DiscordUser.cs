namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/user#user-object
    public class DiscordUser : Identifiable
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// True if the user is a bot and false otherwise.
        /// </summary>
        public bool IsBot { get; set; }

        /// <summary>
        /// Gets the integer value of the discriminator.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return 143.
        /// </example>
        public string? Discriminator { get; set; }

        /// <summary>
        /// Gets the username of the user.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return Foo.
        /// </example>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the username with the discriminator.
        /// </summary>
        /// <example>
        /// Given the user: Foo#0143, this would return Foo#0143.
        /// </example>
        public string FullName()
        {
            var number = Discriminator != null ? int.Parse(Discriminator) : -1;
            return $"{Name}#{number:0000}";
        }

        /// <summary>
        /// Gets the URL for the profile picture.
        /// </summary>
        public string? AvatarUrl { get; set; }
    }
}
