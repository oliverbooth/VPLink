using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using VPLink.Services;

namespace VPLink.Commands;

/// <summary>
///     Represents a class which implements the <c>info</c> command.
/// </summary>
internal sealed class InfoCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BotService _botService;
    private readonly DiscordSocketClient _discordClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoCommand" /> class.
    /// </summary>
    /// <param name="botService">The bot service.</param>
    /// <param name="discordClient"></param>
    public InfoCommand(BotService botService, DiscordSocketClient discordClient)
    {
        _botService = botService;
        _discordClient = discordClient;
    }

    [SlashCommand("info", "Displays information about the bot.")]
    [RequireContext(ContextType.Guild)]
    public async Task InfoAsync()
    {
        SocketGuildUser member = Context.Guild.GetUser(_discordClient.CurrentUser.Id);
        string botVersion = _botService.Version;

        SocketRole? highestRole = member.Roles.Where(r => r.Color != Color.Default).MaxBy(r => r.Position);

        var embed = new EmbedBuilder();
        embed.WithAuthor(member);
        embed.WithColor(highestRole?.Color ?? Color.Default);
        embed.WithThumbnailUrl(member.GetAvatarUrl());
        embed.WithTitle($"VPLink v{botVersion}");
        embed.WithDescription("Created by <@94248427663130624>, hosted [on GitHub](https://github.com/oliverbooth/VPLink).");
        embed.AddField("Ping", $"{_discordClient.Latency} ms", true);
        embed.AddField("Started", $"<t:{_botService.StartedAt.ToUnixTimeSeconds()}:R>", true);

        var builder = new StringBuilder();
        builder.AppendLine($"VPLink: {botVersion}");
        builder.AppendLine($"Discord.Net: {_botService.DiscordNetVersion}");
        builder.AppendLine($"VP#: {_botService.VpSharpVersion}");
        builder.AppendLine($"CLR: {Environment.Version.ToString(3)}");
        builder.AppendLine($"Host: {Environment.OSVersion}");

        embed.AddField("Version", $"```\n{builder}\n```");

        await RespondAsync(embed: embed.Build(), ephemeral: true).ConfigureAwait(false);
    }
}
