using VPLink.Common.Data;

namespace VPLink.Common.Services;

/// <summary>
///     Represents a service that listens for messages from the Virtual Paradise world.
/// </summary>
public interface IVirtualParadiseMessageService : IRelayTarget
{
    /// <summary>
    ///     Gets an observable that is triggered when a valid message is received from the Virtual Paradise world.
    /// </summary>
    /// <value>
    ///     An observable that is triggered when a valid message is received from the Virtual Paradise world.
    /// </value>
    IObservable<RelayedMessage> OnMessageReceived { get; }
}