using Discord;
using VpSharp.Entities;

namespace VpBridge.Services;

/// <summary>
///     Represents a service that sends messages to the Discord channel.
/// </summary>
public interface IDiscordService
{
    /// <summary>
    ///     Gets an observable that is triggered when a message is received from the Discord channel.
    /// </summary>
    /// <value>An observable that is triggered when a message is received from the Discord channel.</value>
    IObservable<IUserMessage> OnMessageReceived { get; }

    /// <summary>
    ///     Sends a message to the Discord channel.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task SendMessageAsync(VirtualParadiseMessage message);
}