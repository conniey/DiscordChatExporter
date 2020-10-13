namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure
    public class EmbedField
    {
        public string? Name { get; set;  }

        public string? Value { get; set;  }

        public bool IsInline { get; set;  }
    }
}
