using System;
using System.Collections.Generic;
using System.Drawing;

namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// Represents an embed within a <see cref="ChannelMessage"/>.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#embed-object"/>
    public class ChannelEmbed
    {
        /// <summary>
        /// Gets or sets the title of embed.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description of embed.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the url of embed.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of embed content.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the color code of the embed.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Gets or sets footer information.
        /// </summary>
        public EmbedFooter? Footer { get; set; }

        /// <summary>
        /// Gets or sets the image information.
        /// </summary>
        public EmbedImage? Image { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail information.
        /// </summary>
        public EmbedImage? Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public EmbedAuthor? Author { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public IReadOnlyList<EmbedField> Fields { get; set; } = new List<EmbedField>().AsReadOnly();

    }
}
