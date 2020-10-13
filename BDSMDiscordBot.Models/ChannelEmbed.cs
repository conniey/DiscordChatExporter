using System;
using System.Collections.Generic;
using System.Drawing;

namespace BDSMDiscordBot.Models
{
    public class ChannelEmbed
    {
        public string? Title { get; set; }

        public string? Url { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public Color? Color { get; set; }

        public EmbedAuthor? Author { get; set; }

        public string? Description { get; set; }

        public IReadOnlyList<EmbedField> Fields { get; set; } = new List<EmbedField>().AsReadOnly();

        public EmbedImage? Thumbnail { get; set; }

        public EmbedImage? Image { get; set; }

        public EmbedFooter? Footer { get; set; }
    }
}
