using VPLink.Common.Configuration;

namespace VPLink.Configuration;

/// <inheritdoc />
internal sealed class DiscordConfiguration : IDiscordConfiguration
{
    /// <inheritdoc />
    public ulong ChannelId { get; set; }

    /// <inheritdoc />
    public string Token { get; set; } = string.Empty;
}
