namespace VPLink.Data;

/// <summary>
///     Represents a message that is relayed between Discord and Virtual Paradise.
/// </summary>
public readonly struct RelayedMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelayedMessage" /> struct.
    /// </summary>
    /// <param name="author">The author.</param>
    /// <param name="content">The content.</param>
    public RelayedMessage(string author, string content)
    {
        Author = author;
        Content = content;
    }

    /// <summary>
    ///     Gets the message content.
    /// </summary>
    /// <value>The message content.</value>
    public string Content { get; }

    /// <summary>
    ///     Gets the user that sent the message.
    /// </summary>
    /// <value>The user that sent the message.</value>
    public string Author { get; }
}
