using System;
using System.Collections.Generic;
using System.Text;
using BDSMDiscordBot.Models;

namespace BDSMDiscordBot.Models
{
    public class ExportedLog
    {
        public DiscordGuild Guild { get; set; }

        public DiscordChannel Channel { get; set; }

        public ExportDateRange DateRange { get; set; }

        public List<ChannelMessage> Messages { get; set; } = new List<ChannelMessage>();

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
