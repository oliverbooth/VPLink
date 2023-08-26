using VPLink.Common.Configuration;

namespace VPLink.Configuration;

/// <inheritdoc />
internal sealed class VirtualParadiseConfiguration : IVirtualParadiseConfiguration
{
    /// <inheritdoc />
    public string BotName { get; set; } = "VPLink";

    /// <inheritdoc />
    public IChatConfiguration Chat { get; } = new ChatConfiguration();

    /// <inheritdoc />
    public string Password { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Username { get; set; } = string.Empty;

    /// <inheritdoc />
    public string World { get; set; } = string.Empty;
}
