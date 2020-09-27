namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/user#user-object
    public class DiscordUser : Identifiable
    {
        public string? Id { get; set; }

        public bool IsBot { get; set; }

        public string? Discriminator { get; set; }

        public string? Name { get; set; }

        public string FullName => $"{Name}#{Discriminator:0000}";

        public string? AvatarUrl { get; set; }
    }
}
