namespace BDSMDiscordBot.Models
{
    /// <summary>
    /// An identifiable object.
    /// </summary>
    interface Identifiable
    {
        /// <summary>
        /// Gets the unique identifier for the object. The identifier is not globally unique. Only unique with the same
        /// class of objects.
        /// </summary>
        string? Id { get; }
    }
}
