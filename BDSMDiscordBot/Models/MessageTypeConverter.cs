using DiscordChatExporter.Domain.Discord.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordImporterBot.Models
{
    class MessageTypeConverter : JsonConverter<MessageType>
    {
        public override MessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var type = reader.GetString();

            return Enum.TryParse<MessageType>(type, out var result)
                ? result
                : throw new NotSupportedException($"{type} is not supported");
        }

        public override void Write(Utf8JsonWriter writer, MessageType value, JsonSerializerOptions options) 
            => writer.WriteStringValue(value.ToString());
    }
}
