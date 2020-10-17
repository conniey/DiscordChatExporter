using System;

namespace BDSMDiscordBot.Models
{
    public class ExportDateRange
    {
        public DateTimeOffset? After { get; set; }

        public DateTimeOffset? Before { get; set; }
    }
}
