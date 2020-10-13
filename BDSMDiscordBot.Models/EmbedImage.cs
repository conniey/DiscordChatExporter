namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// An image within a <see cref="ChannelEmbed"/>'s embed.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#embed-object-embed-image-structure"/>
    public class EmbedImage
    {
        /// <summary>
        /// Gets or sets the source url of image (only supports http(s) and attachments).
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the width of image.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of image.
        /// </summary>
        public int? Height { get; set; }
    }
}
