using Discord;
using VpSharp.Entities;

namespace VPLink.Services;

/// <summary>
///     Represents a service that sends messages to the Virtual Paradise world server.
/// </summary>
public interface IVirtualParadiseService
{
    /// <summary>
    ///     Gets an observable that is triggered when a message is received from the Virtual Paradise world server.
    /// </summary>
    /// <value>
    ///     An observable that is triggered when a message is received from the Virtual Paradise world server.
    /// </value>
    IObservable<VirtualParadiseMessage> OnMessageReceived { get; }

    /// <summary>
    ///     Sends a message to the Virtual Paradise world server.
    /// </summary>
    /// <param name="message">The Discord message to send.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task SendMessageAsync(IUserMessage message);
}
