namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#reaction-object
    public class ChannelReaction : Identifiable
    {
        /// <summary>
        /// Gets the identifier for the emoji.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets the name of the emoji.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether it is animated.
        /// </summary>
        public bool IsAnimated { get; set; }

        /// <summary>
        /// Gets or sets the emoji URL.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of times this was reacted to.
        /// </summary>
        public int Count { get; set; }

        public override string ToString() => $"{Name} ({Count})";
    }
}
