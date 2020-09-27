using DiscordChatExporter.Domain.Discord.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordImporterBot.Models
{
    public class ExportedLog
    {
        public Guild Guild { get; set; }

        public Channel Channel { get; set; }

        public ExportDateRange DateRange { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>();

        public int MessageCount { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Total #: " + MessageCount);
            builder.AppendLine("Messages:");
            builder.AppendLine();

            foreach(var message in Messages)
            {
                builder.AppendFormat($"[{message.Timestamp:r}] {message.Content}");
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
