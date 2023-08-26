using VPLink.Common.Configuration;

namespace VPLink.Common.Services;

/// <summary>
///     Represents the configuration service.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    ///     Gets the bot configuration.
    /// </summary>
    /// <value>The bot configuration.</value>
    IBotConfiguration BotConfiguration { get; }

    /// <summary>
    ///     Gets the Discord configuration.
    /// </summary>
    /// <value>The Discord configuration.</value>
    IDiscordConfiguration DiscordConfiguration { get; }

    /// <summary>
    ///     Gets the Virtual Paradise configuration.
    /// </summary>
    /// <value>The Virtual Paradise configuration.</value>
    IVirtualParadiseConfiguration VirtualParadiseConfiguration { get; }
}
