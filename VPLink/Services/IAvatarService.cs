using VpSharp.Entities;

namespace VPLink.Services;

/// <summary>
///     Represents a service that listens for, and triggers, avatar events.
/// </summary>
public interface IAvatarService
{
    /// <summary>
    ///     Gets an observable that is triggered when an avatar enters the Virtual Paradise world.
    /// </summary>
    /// <value>
    ///     An observable that is triggered when an avatar enters the Virtual Paradise world.
    /// </value>
    IObservable<VirtualParadiseAvatar> OnAvatarJoined { get; }

    /// <summary>
    ///     Gets an observable that is triggered when an avatar exits the Virtual Paradise world.
    /// </summary>
    /// <value>
    ///     An observable that is triggered when an avatar exits the Virtual Paradise world.
    /// </value>
    IObservable<VirtualParadiseAvatar> OnAvatarLeft { get; }
}
