namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-image-structure
    public class EmbedImage
    {
        public string? Url { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }
    }
}
