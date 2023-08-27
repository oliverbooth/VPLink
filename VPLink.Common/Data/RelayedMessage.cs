namespace VPLink.Common.Data;

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
    /// <param name="isReply">A value indicating whether this message is a reply.</param>
    public RelayedMessage(string? author, string content, bool isReply)
    {
        Author = author;
        Content = content;
        IsReply = isReply;
    }

    /// <summary>
    ///     Gets the user that sent the message.
    /// </summary>
    /// <value>The user that sent the message.</value>
    public string? Author { get; }

    /// <summary>
    ///     Gets the message content.
    /// </summary>
    /// <value>The message content.</value>
    public string Content { get; }

    /// <summary>
    ///     Gets a value indicating whether this message is a reply.
    /// </summary>
    /// <value><see langword="true" /> if this message is a reply; otherwise, <see langword="false" />.</value>
    public bool IsReply { get; }
}
