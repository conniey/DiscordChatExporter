namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#message-object-message-types
    public enum ChannelMessageType
    {
        Default,
        RecipientAdd,
        RecipientRemove,
        Call,
        ChannelNameChange,
        ChannelIconChange,
        ChannelPinnedMessage,
        GuildMemberJoin
    }
}
