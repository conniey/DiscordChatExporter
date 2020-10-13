namespace BDSMDiscordBot.Models
{
    public class DiscordChannel : Identifiable
    {
        public string? Id { get; set; }

        public ChannelType? Type { get; set; }

        public bool IsTextChannel =>
            Type == ChannelType.GuildTextChat ||
            Type == ChannelType.DirectMessage ||
            Type == ChannelType.GroupDirectMessage||
            Type == ChannelType.GuildNews ||
            Type == ChannelType.GuildStore;

        public string? GuildId { get; set; }

        public string? Category { get; set; }

        public string? Name { get; set; }

        public string? Topic { get; set; }
    }
}
