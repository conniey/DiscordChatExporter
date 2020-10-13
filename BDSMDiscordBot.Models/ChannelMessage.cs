using System;
using System.Collections.Generic;
using System.Linq;

namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#message-object
    public class ChannelMessage : Identifiable
    {
        public string? Id { get; set; }

        public ChannelMessageType Type { get; set; }

        public DiscordUser? Author { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset? EditedTimestamp { get; set; }

        public DateTimeOffset? CallEndedTimestamp { get; set; }

        public bool IsPinned { get; set; }

        public string? Content { get; set; }

        public IReadOnlyList<ChannelAttachment> Attachments { get; set; } = new List<ChannelAttachment>().AsReadOnly();

        public IReadOnlyList<ChannelEmbed> Embeds { get; set; } = new List<ChannelEmbed>().AsReadOnly();

        public IReadOnlyList<ChannelReaction> Reactions { get; set; } = new List<ChannelReaction>().AsReadOnly();

        public IReadOnlyList<DiscordUser> MentionedUsers { get; set; } = new List<DiscordUser>().AsReadOnly();
    }
}
