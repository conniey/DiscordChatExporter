using System;

namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#message-object
    public class ChannelMessage : Identifiable
    {
        public string Id { get; set; }

        public ChannelMessageType Type { get; set; }

        public User Author { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public DateTimeOffset? EditedTimestamp { get; set; }

        public DateTimeOffset? CallEndedTimestamp { get; set; }

        public bool IsPinned { get; set; }

        public string Content { get; set; }

        public IReadOnlyList<ChannelAttachment> Attachments { get; set; }

        public IReadOnlyList<Embed> Embeds { get; set; }

        public IReadOnlyList<Reaction> Reactions { get; set; }

        public IReadOnlyList<User> MentionedUsers { get; set; }

        public ChannelMessage()
        {
        }

    }
