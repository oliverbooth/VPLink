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
}
