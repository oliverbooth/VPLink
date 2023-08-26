namespace VPLink.Common.Configuration;

/// <summary>
///     Represents the Virtual Paradise configuration.
/// </summary>
public interface IVirtualParadiseConfiguration
{
    /// <summary>
    ///     Gets or sets the display name of the bot.
    /// </summary>
    /// <value>The display name.</value>
    string BotName { get; set; }

    /// <summary>
    ///     Gets or sets the chat configuration.
    /// </summary>
    /// <value>The chat configuration.</value>
    IChatConfiguration Chat { get; }

    /// <summary>
    ///     Gets or sets the password with which to log in to Virtual Paradise.
    /// </summary>
    /// <value>The login password.</value>
    string Password { get; set; }

    /// <summary>
    ///     Gets or sets the username with which to log in to Virtual Paradise.
    /// </summary>
    /// <value>The login username.</value>
    string Username { get; set; }

    /// <summary>
    ///     Gets or sets the world into which the bot should enter.
    /// </summary>
    /// <value>The world to enter.</value>
    string World { get; set; }
}
