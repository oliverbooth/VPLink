using VpSharp;

namespace VPLink.Configuration;

/// <summary>
///     Represents the chat configuration.
/// </summary>
public sealed class ChatConfiguration
{
    /// <summary>
    ///     Gets or sets the color of the message.
    /// </summary>
    /// <value>The message color.</value>
    public uint Color { get; set; } = 0x191970;

    /// <summary>
    ///     Gets or sets the font style of the message.
    /// </summary>
    /// <value>The font style.</value>
    public FontStyle Style { get; set; } = FontStyle.Regular;
}
