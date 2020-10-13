using System;
using System.Collections.Generic;
using System.IO;

namespace BDSMDiscordBot.Models
{
    // https://discord.com/developers/docs/resources/channel#attachment-object
    public class ChannelAttachment : Identifiable
    {
        private static readonly HashSet<string> s_imageFileExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public string? Id { get; set; }

        public string? Url { get; set; }

        public string? FileName { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? SizeInBytes { get; set; }

        public bool IsImage => FileName != null ? s_imageFileExtensions.Contains(Path.GetExtension(FileName)) : false;

        public bool IsSpoiler => FileName != null && IsImage
            && FileName.StartsWith("SPOILER_", StringComparison.Ordinal);
    }
}
