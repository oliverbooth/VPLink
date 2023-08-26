namespace VPLink.Common.Configuration;

/// <summary>
///     Represents the bot configuration.
/// </summary>
public interface IBotConfiguration
{
    /// <summary>
    ///     Gets a value indicating whether the bot should announce avatar events.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should announce avatar events; otherwise, <see langword="false" />.
    /// </value>
    bool AnnounceAvatarEvents { get; }

    /// <summary>
    ///     Gets a value indicating whether the bot should announce avatar events for bots.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should announce avatar events for bots; otherwise,
    ///     <see langword="false" />.
    /// </value>
    bool AnnounceBots { get; }

    /// <summary>
    ///     Gets a value indicating whether the bot should relay messages from other bots.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should relay messages from other bots; otherwise,
    ///     <see langword="false" />.
    /// </value>
    bool RelayBotMessages { get; }
}
