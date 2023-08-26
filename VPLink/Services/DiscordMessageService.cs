using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Common.Configuration;
using VPLink.Common.Data;
using VPLink.Common.Services;
using VpSharp.Entities;
using IUser = Discord.IUser;

namespace VPLink.Services;

/// <inheritdoc cref="IDiscordMessageService" />
internal sealed partial class DiscordMessageService : BackgroundService, IDiscordMessageService
{
    private static readonly Encoding Utf8Encoding = new UTF8Encoding(false, false);
    private static readonly Regex UnescapeRegex = GetUnescapeRegex();
    private static readonly Regex EscapeRegex = GetEscapeRegex();

    private readonly ILogger<DiscordMessageService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly DiscordSocketClient _discordClient;
    private readonly Subject<RelayedMessage> _messageReceived = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordMessageService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="discordClient">The Discord client.</param>
    public DiscordMessageService(ILogger<DiscordMessageService> logger,
        IConfigurationService configurationService,
        DiscordSocketClient discordClient)
    {
        _logger = logger;
        _configurationService = configurationService;
        _discordClient = discordClient;
    }

    /// <inheritdoc />
    public IObservable<RelayedMessage> OnMessageReceived => _messageReceived.AsObservable();

    /// <inheritdoc />
    public Task AnnounceArrival(VirtualParadiseAvatar avatar)
    {
        if (avatar is null) throw new ArgumentNullException(nameof(avatar));
        if (!TryGetRelayChannel(out ITextChannel? channel)) return Task.CompletedTask;

        var embed = new EmbedBuilder();
        embed.WithColor(0x00FF00);
        embed.WithDescription($"📥 **Avatar Joined**: {avatar.Name} (User #{avatar.User.Id})");

        return channel.SendMessageAsync(embed: embed.Build());
    }

    /// <inheritdoc />
    public Task AnnounceDeparture(VirtualParadiseAvatar avatar)
    {
        if (avatar is null) throw new ArgumentNullException(nameof(avatar));
        if (!TryGetRelayChannel(out ITextChannel? channel)) return Task.CompletedTask;

        var embed = new EmbedBuilder();
        embed.WithColor(0xFF0000);
        embed.WithDescription($"📤 **Avatar Left**: {avatar.Name} (User #{avatar.User.Id})");

        return channel.SendMessageAsync(embed: embed.Build());
    }

    /// <inheritdoc />
    public Task SendMessageAsync(RelayedMessage message)
    {
        if (!TryGetRelayChannel(out ITextChannel? channel)) return Task.CompletedTask;
        return channel.SendMessageAsync($"**{message.Author}**: {message.Content}");
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordClient.MessageReceived += OnDiscordMessageReceived;
        return Task.CompletedTask;
    }

    private Task OnDiscordMessageReceived(SocketMessage arg)
    {
        if (arg is not IUserMessage message)
            return Task.CompletedTask;

        if (message.Author is not IGuildUser author)
            return Task.CompletedTask;

        if (author.Id == _discordClient.CurrentUser.Id)
            return Task.CompletedTask;

        if (author.IsBot && !_configurationService.BotConfiguration.RelayBotMessages)
            return Task.CompletedTask;

        if (message.Channel.Id != _configurationService.DiscordConfiguration.ChannelId)
            return Task.CompletedTask;

        string displayName = author.Nickname ?? author.GlobalName ?? author.Username;
        string unescaped = UnescapeRegex.Replace(message.Content, "$1");
        string content = EscapeRegex.Replace(unescaped, "\\$1");

        IReadOnlyCollection<IAttachment> attachments = message.Attachments;
        if (attachments.Count > 0)
        {
            using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
            for (var index = 0; index < attachments.Count; index++)
            {
                builder.AppendLine(attachments.ElementAt(index).Url);
            }

            // += allocates more than necessary, just interpolate
            content = $"{content}\n{builder}";
        }

        content = content.Trim();
        _logger.LogInformation("Message by {Author}: {Content}", author, content);

        Span<byte> buffer = stackalloc byte[255]; // VP message length limit
        var messages = new List<RelayedMessage>();
        int byteCount = Utf8Encoding.GetByteCount(content);

        var offset = 0;
        while (offset < byteCount)
        {
            int length = Math.Min(byteCount - offset, 255);
            Utf8Encoding.GetBytes(content.AsSpan(offset, length), buffer);
            messages.Add(new RelayedMessage(displayName, Utf8Encoding.GetString(buffer)));
            offset += length;
        }

        messages.ForEach(_messageReceived.OnNext);
        return Task.CompletedTask;
    }

    private bool TryGetRelayChannel([NotNullWhen(true)] out ITextChannel? channel)
    {
        IDiscordConfiguration configuration = _configurationService.DiscordConfiguration;
        ulong channelId = configuration.ChannelId;

        if (_discordClient.GetChannel(channelId) is ITextChannel textChannel)
        {
            channel = textChannel;
            return true;
        }

        _logger.LogError("Channel {ChannelId} does not exist", channelId);
        channel = null;
        return false;
    }

    [GeneratedRegex(@"\\(\*|_|`|~|\\)", RegexOptions.Compiled)]
    private static partial Regex GetUnescapeRegex();

    [GeneratedRegex(@"(\*|_|`|~|\\)", RegexOptions.Compiled)]
    private static partial Regex GetEscapeRegex();
}
