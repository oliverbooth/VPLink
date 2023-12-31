using VPLink.Common.Configuration;
using VpSharp;

namespace VPLink.Configuration;

/// <inheritdoc />
internal sealed class ChatConfiguration : IChatConfiguration
{
    /// <inheritdoc />
    public uint Color { get; set; } = 0x191970;

    /// <inheritdoc />
    public FontStyle Style { get; set; } = FontStyle.Regular;

    /// <inheritdoc />
    public uint ReplyColor { get; set; } = 0x808080;

    /// <inheritdoc />
    public FontStyle ReplyStyle { get; set; } = FontStyle.Italic;
}
