namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// The author of a <see cref="ChannelEmbed"/>.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#embed-object-embed-author-structure"/>
    public class EmbedAuthor
    {
        /// <summary>
        /// Gets or sets the name of author.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the url of the author.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the url of author icon (only supports http(s) and attachments).
        /// </summary>
        public string? IconUrl { get; set; }
    }
}
