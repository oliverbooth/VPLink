using VPLink.Data;

namespace VPLink.Services;

/// <summary>
///     Represents an object that can be used as a relay target.
/// </summary>
public interface IRelayTarget
{
    /// <summary>
    ///     Sends a message to the relay target.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task SendMessageAsync(RelayedMessage message);
}
