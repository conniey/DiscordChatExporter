using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BDSMDiscordBot.Models;

namespace BDSMDiscordBot.Models
{
    class MessageTypeConverter : JsonConverter<ChannelMessageType>
    {
        public override ChannelMessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var type = reader.GetString();

            return Enum.TryParse<ChannelMessageType>(type, out var result)
                ? result
                : throw new NotSupportedException($"{type} is not supported");
        }

        public override void Write(Utf8JsonWriter writer, ChannelMessageType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
