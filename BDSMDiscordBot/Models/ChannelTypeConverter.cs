using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BDSMDiscordBot.Models;

namespace BDSMDiscordBot.Models
{
    public class ChannelTypeConverter : JsonConverter<ChannelType>
    {
        public override ChannelType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var channelType = reader.GetString();

            return Enum.TryParse<ChannelType>(channelType, out var result)
                ? result
                : throw new NotSupportedException($"{channelType} is not supported");
        }

        public override void Write(Utf8JsonWriter writer, ChannelType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
