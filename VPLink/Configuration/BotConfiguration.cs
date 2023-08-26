namespace VPLink.Configuration;

/// <summary>
///     Represents the bot configuration.
/// </summary>
public sealed class BotConfiguration
{
    /// <summary>
    ///     Gets or sets a value indicating whether the bot should announce avatar events.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should announce avatar events; otherwise, <see langword="false" />.
    /// </value>
    public bool AnnounceAvatarEvents { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the bot should announce avatar events for bots.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should announce avatar events for bots; otherwise,
    ///     <see langword="false" />.
    /// </value>
    public bool AnnounceBots { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the bot should relay messages from other bots.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the bot should relay messages from other bots; otherwise,
    ///     <see langword="false" />.
    /// </value>
    public bool RelayBotMessages { get; set; } = false;
}
