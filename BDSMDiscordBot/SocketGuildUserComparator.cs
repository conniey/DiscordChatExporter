using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Discord.WebSocket;

namespace DiscordImporterBot
{
    public class SocketGuildUserComparator : IEqualityComparer<SocketGuildUser>
    {
        public static readonly SocketGuildUserComparator Instance = new SocketGuildUserComparator();

        public bool Equals([AllowNull] SocketGuildUser x, [AllowNull] SocketGuildUser y)
        {
            if (x == null)
            {
                return y == null;
            }
            if (y == null)
            {
                return x == null;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] SocketGuildUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
