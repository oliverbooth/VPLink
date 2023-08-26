using VPLink.Data;
using VpSharp.Entities;

namespace VPLink.Services;

/// <summary>
///     Represents a service that listens for messages from the Discord bridge channel.
/// </summary>
public interface IDiscordMessageService : IRelayTarget
{
    /// <summary>
    ///     Gets an observable that is triggered when a valid message is received from the Discord bridge channel.
    /// </summary>
    /// <value>
    ///     An observable that is triggered when a valid message is received from the Discord bridge channel.
    /// </value>
    IObservable<RelayedMessage> OnMessageReceived { get; }

    /// <summary>
    ///     Announces the arrival of an avatar.
    /// </summary>
    /// <param name="avatar">The avatar.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task AnnounceArrival(VirtualParadiseAvatar avatar);

    /// <summary>
    ///     Announces the arrival of an avatar.
    /// </summary>
    /// <param name="avatar">The avatar.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    Task AnnounceDeparture(VirtualParadiseAvatar avatar);
}
