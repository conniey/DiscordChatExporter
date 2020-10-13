namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// The field associated with a <see cref="ChannelEmbed"/>.
    /// </summary>
    /// <seealso cref="https://discord.com/developers/docs/resources/channel#embed-object-embed-field-structure"/>
    public class EmbedField
    {
        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string? Name { get; set;  }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public string? Value { get; set;  }

        /// <summary>
        /// Gets or sets whether or not this field should display inline.
        /// </summary>
        public bool IsInline { get; set;  }
    }
}
