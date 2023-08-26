namespace VPLink.Configuration;

/// <summary>
///     Represents the Virtual Paradise configuration.
/// </summary>
public sealed class VirtualParadiseConfiguration
{
    /// <summary>
    ///     Gets or sets the display name of the bot.
    /// </summary>
    /// <value>The display name.</value>
    public string BotName { get; set; } = "VPLink";

    /// <summary>
    ///     Gets or sets the chat configuration.
    /// </summary>
    /// <value>The chat configuration.</value>
    public ChatConfiguration ChatConfiguration { get; } = new();

    /// <summary>
    ///     Gets or sets the password with which to log in to Virtual Paradise.
    /// </summary>
    /// <value>The login password.</value>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username with which to log in to Virtual Paradise.
    /// </summary>
    /// <value>The login username.</value>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the world into which the bot should enter.
    /// </summary>
    /// <value>The world to enter.</value>
    public string World { get; set; } = string.Empty;
}
