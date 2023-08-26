namespace VPLink.Common.Configuration;

/// <summary>
///     Represents the Discord configuration.
/// </summary>
public interface IDiscordConfiguration
{
    /// <summary>
    ///     Gets the channel ID to which the bot should relay messages.
    /// </summary>
    /// <value>The channel ID.</value>
    ulong ChannelId { get; }

    /// <summary>
    ///     Gets the Discord token.
    /// </summary>
    /// <value>The Discord token.</value>
    string Token { get; }
}
