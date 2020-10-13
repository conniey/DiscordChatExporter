namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// The footer associated with a <seealso cref="ChannelEmbed"/>.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure"/>
    public class EmbedFooter
    {
        /// <summary>
        /// Gets or sets the footer text.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the url of footer icon (only supports http(s) and attachments)
        /// </summary>
        public string? IconUrl { get; set; }
    }
}
