using VPLink.Configuration;

namespace VPLink.Services;

/// <summary>
///     Represents the configuration service.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    ///     Gets the bot configuration.
    /// </summary>
    /// <value>The bot configuration.</value>
    BotConfiguration BotConfiguration { get; }

    /// <summary>
    ///     Gets the Discord configuration.
    /// </summary>
    /// <value>The Discord configuration.</value>
    DiscordConfiguration DiscordConfiguration { get; }

    /// <summary>
    ///     Gets the Virtual Paradise configuration.
    /// </summary>
    /// <value>The Virtual Paradise configuration.</value>
    VirtualParadiseConfiguration VirtualParadiseConfiguration { get; }
}
