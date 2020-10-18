using System;
using Discord.WebSocket;

namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// Extension methods for users.
    /// </summary>
    public static class SocketGuildUserExtensions
    {
        /// <summary>
        /// Figures out if the <paramref name="user"/> has joined longer than the given date <paramref name="date"/>.
        /// </summary>
        /// <param name="user">User to compare.</param>
        /// <param name="date">Cut off date.</param>
        /// <returns>True if the user is not a bot and has joined later than the given date.</returns>
        public static bool IsJoinDateOlderThan(this SocketGuildUser user, DateTimeOffset date)
        {
            return !user.IsBot && user.JoinedAt.HasValue && user.JoinedAt.Value.CompareTo(date) == -1;
        }

        public static string ToDisplayName(this SocketGuildUser user) => $"{user.Username}:{user.Discriminator}";
    }
}
