using Cysharp.Text;
using Discord;
using Discord.Interactions;
using VpSharp;
using VpSharp.Entities;

namespace VPLink.Commands;

/// <summary>
///     Represents a class which implements the <c>who</c> command.
/// </summary>
internal sealed class WhoCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly VirtualParadiseClient _virtualParadiseClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WhoCommand" /> class.
    /// </summary>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public WhoCommand(VirtualParadiseClient virtualParadiseClient)
    {
        _virtualParadiseClient = virtualParadiseClient;
    }

    [SlashCommand("who", "Displays a list of active users in Virtual Paradise.")]
    [RequireContext(ContextType.Guild)]
    public async Task HandleAsync()
    {
        var embed = new EmbedBuilder();
        embed.WithColor(0x1E88E5);
        embed.WithAuthor($"🌎 {_virtualParadiseClient.CurrentWorld?.Name}");
        embed.WithTitle("Active Users");
        embed.WithTimestamp(DateTimeOffset.UtcNow);

        using Utf8ValueStringBuilder userBuilder = ZString.CreateUtf8StringBuilder();
        using Utf8ValueStringBuilder botsBuilder = ZString.CreateUtf8StringBuilder();
        var userCount = 0;
        var botCount = 0;

        foreach (VirtualParadiseAvatar avatar in _virtualParadiseClient.Avatars)
        {
            if (avatar.IsBot)
            {
                botsBuilder.AppendLine($"* {avatar.Name} ({avatar.User.Id})");
                botCount++;
            }
            else
            {
                userBuilder.AppendLine($"* {avatar.Name} ({avatar.User.Id})");
                userCount++;
            }
        }

        string userTitle = userCount switch
        {
            0 => "Users",
            1 => "1 User",
            _ => $"{userCount} Users"
        };

        string botTitle = botCount switch
        {
            0 => "Bots",
            1 => "1 Bot",
            _ => $"{botCount} Bots"
        };

        embed.AddField($"👤 {userTitle}", userCount > 0 ? userBuilder.ToString() : "*None*", true);
        embed.AddField($"🤖 {botTitle}", botCount > 0 ? botsBuilder.ToString() : "*None*", true);

        await RespondAsync(embed: embed.Build());
    }
}
