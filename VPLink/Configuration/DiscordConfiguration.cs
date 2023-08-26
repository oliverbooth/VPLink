namespace VPLink.Configuration;

/// <summary>
///     Represents the Discord configuration.
/// </summary>
public sealed class DiscordConfiguration
{
    /// <summary>
    ///     Gets or sets the channel ID to which the bot should relay messages.
    /// </summary>
    /// <value>The channel ID.</value>
    public ulong ChannelId { get; set; }

    /// <summary>
    ///     Gets or sets the Discord token.
    /// </summary>
    /// <value>The Discord token.</value>
    public string Token { get; set; } = string.Empty;
}
