using VpSharp;

namespace VPLink.Common.Configuration;

/// <summary>
///     Represents the chat configuration.
/// </summary>
public interface IChatConfiguration
{
    /// <summary>
    ///     Gets or sets the color of the message.
    /// </summary>
    /// <value>The message color.</value>
    uint Color { get; set; }

    /// <summary>
    ///     Gets or sets the font style of the message.
    /// </summary>
    /// <value>The font style.</value>
    FontStyle Style { get; set; }

    /// <summary>
    ///     Gets or sets the color of a reply message.
    /// </summary>
    /// <value>The reply message color.</value>
    uint ReplyColor { get; set; }

    /// <summary>
    ///     Gets or sets the font style of a reply message.
    /// </summary>
    /// <value>The reply font style.</value>
    FontStyle ReplyStyle { get; set; }
}
