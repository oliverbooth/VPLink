using VPLink.Common.Configuration;

namespace VPLink.Configuration;

/// <inheritdoc />
internal sealed class BotConfiguration : IBotConfiguration
{
    /// <inheritdoc />
    public bool AnnounceAvatarEvents { get; set; } = true;

    /// <inheritdoc />
    public bool AnnounceBots { get; set; } = false;

    /// <inheritdoc />
    public bool RelayBotMessages { get; set; } = false;
}
