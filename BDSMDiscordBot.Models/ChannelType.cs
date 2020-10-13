namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// Types of channels in Discord.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#channel-object-channel-types"/>
    public enum ChannelType
    {
        /// <summary>
        /// a text channel within a server
        /// </summary>
        GuildTextChat = 0,

        /// <summary>
        /// a direct message between users
        /// </summary>
        DirectMessage = 1,

        /// <summary>
        /// a voice channel within a server
        /// </summary>
        GuildVoiceChat = 2,

        /// <summary>
        /// a direct message between multiple users
        /// </summary>
        GroupDirectMessage = 3,

        /// <summary>
        /// an organizational category that contains up to 50 channels.
        /// <seealso cref="https://support.discord.com/hc/en-us/articles/115001580171-Channel-Categories-101"/>
        /// </summary>
        GuildCategory = 4,

        /// <summary>
        /// a channel that users can follow and crosspost into their own server.
        /// <seealso cref="https://support.discord.com/hc/en-us/articles/360032008192"/>
        /// </summary>
        GuildNews = 5,

        /// <summary>
        /// a channel in which game developers can sell their game on Discord.
        /// <seealso cref="https://discord.com/developers/docs/game-and-server-management/special-channels"/>
        /// </summary>
        GuildStore = 6,
    }
}
