using Discord;

namespace VPLink.Common.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="IUser" /> interface.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    ///     Gets the display name of the user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The display name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="user" /> is <c>null</c>.</exception>
    public static string GetDisplayName(this IUser user)
    {
        string displayName = user switch
        {
            null => throw new ArgumentNullException(nameof(user)),
            IGuildUser member => member.Nickname ?? member.GlobalName ?? member.Username,
            _ => user.GlobalName ?? user.Username
        };
        
        return user.IsBot ? $"[{displayName}]" : displayName;
    }
}
