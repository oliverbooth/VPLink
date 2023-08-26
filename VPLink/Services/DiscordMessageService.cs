using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Cysharp.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Common.Configuration;
using VPLink.Common.Data;
using VPLink.Common.Services;
using VpSharp.Entities;

namespace VPLink.Services;

/// <inheritdoc cref="IDiscordMessageService" />
internal sealed partial class DiscordMessageService : BackgroundService, IDiscordMessageService
{
    private static readonly Encoding Utf8Encoding = new UTF8Encoding(false, false);

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
        if (!ValidateMessage(arg, out IUserMessage? message, out IGuildUser? author))
            return Task.CompletedTask;

        string displayName = author.Nickname ?? author.GlobalName ?? author.Username;
        string content = message.Content;

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

    private bool ValidateMessage(SocketMessage socketMessage,
        [NotNullWhen(true)] out IUserMessage? message,
        [NotNullWhen(true)] out IGuildUser? author)
    {
        message = socketMessage as IUserMessage;
        if (message is null)
        {
            author = null;
            return false;
        }

        author = message.Author as IGuildUser;
        if (author is null)
        {
            message = null;
            return false;
        }

        if (author.Id == _discordClient.CurrentUser.Id)
        {
            author = null;
            message = null;
            return false;
        }

        if (author.IsBot && !_configurationService.BotConfiguration.RelayBotMessages)
        {
            author = null;
            message = null;
            return false;
        }

        if (message.Channel.Id != _configurationService.DiscordConfiguration.ChannelId)
        {
            author = null;
            message = null;
            return false;
        }

        return true;
    }
}
