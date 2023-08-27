using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Cysharp.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Common;
using VPLink.Common.Configuration;
using VPLink.Common.Data;
using VPLink.Common.Extensions;
using VPLink.Common.Services;
using VpSharp.Entities;

namespace VPLink.Services;

/// <inheritdoc cref="IDiscordMessageService" />
internal sealed class DiscordMessageService : BackgroundService, IDiscordMessageService
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

        string displayName = author.GetDisplayName();
        var builder = new PlainTextMessageBuilder();

        IGuild guild = author.Guild;
        SanitizeContent(guild, message.Content, ref builder);
        var content = builder.ToString();

        _logger.LogInformation("Message by {Author}: {Content}", author, content);

        var messages = new List<RelayedMessage>();

        MessageReference reference = arg.Reference;
        if (reference?.MessageId.IsSpecified == true)
        {
            string? replyContent = GetReplyContent(arg, reference, out IUserMessage? fetchedMessage);
            if (replyContent is not null)
            {
                IUser replyAuthor = fetchedMessage!.Author;
                string name = fetchedMessage.Author.GetDisplayName();

                _logger.LogInformation("Replying to {Author}: {Content}", replyAuthor, replyContent);
                builder.Clear();
                SanitizeContent(guild, replyContent, ref builder);
                replyContent = builder.ToString();

                messages.Add(new RelayedMessage(null!, $"↩️ Replying to {name}:", true));
                messages.Add(new RelayedMessage(null!, replyContent, true));
            }
        }

        if (arg.Interaction is { Type: InteractionType.ApplicationCommand } interaction)
        {
            string name = interaction.User.GetDisplayName();
            string commandName = interaction.Name;
            messages.Add(new RelayedMessage(null, $"⌨️ {name} used /{commandName}", true));
        }

        AddMessage(messages, displayName, content);

        IReadOnlyCollection<IAttachment> attachments = message.Attachments;
        foreach (IAttachment attachment in attachments)
        {
            messages.Add(new RelayedMessage(displayName, attachment.Url, false));
        }

        messages.ForEach(_messageReceived.OnNext);
        builder.Dispose();
        return Task.CompletedTask;
    }

    private static void AddMessage(ICollection<RelayedMessage> messages, string displayName, string content)
    {
        Span<byte> buffer = stackalloc byte[255]; // VP message length limit
        int byteCount = Utf8Encoding.GetByteCount(content);
        var offset = 0;
        while (offset < byteCount)
        {
            int length = Math.Min(byteCount - offset, 255);
            Utf8Encoding.GetBytes(content.AsSpan(offset, length), buffer);
            messages.Add(new RelayedMessage(displayName, Utf8Encoding.GetString(buffer), false));
            offset += length;
        }
    }

    private string? GetReplyContent(SocketMessage message, MessageReference reference, out IUserMessage? fetchedMessage)
    {
        fetchedMessage = null;
        IGuild authorGuild = ((IGuildUser)message.Author).Guild;
        IGuild guild = authorGuild;

        Optional<ulong> referenceGuildId = reference.GuildId;
        Optional<ulong> referenceMessageId = reference.MessageId;

        if (!referenceMessageId.IsSpecified)
        {
            return null;
        }

        if (referenceGuildId.IsSpecified)
        {
            guild = _discordClient.GetGuild(referenceGuildId.Value) ?? authorGuild;
        }

        ulong referenceChannelId = reference.ChannelId;

        if (!referenceMessageId.IsSpecified)
        {
            return null;
        }

        if (guild.GetChannelAsync(referenceChannelId).GetAwaiter().GetResult() is not ITextChannel channel)
        {
            return null;
        }

        IMessage? referencedMessage = channel.GetMessageAsync(referenceMessageId.Value).GetAwaiter().GetResult();
        if (referencedMessage is null)
        {
            return null;
        }

        fetchedMessage = referencedMessage as IUserMessage;
        string? content = referencedMessage.Content;
        return string.IsNullOrWhiteSpace(content) ? null : content;
    }

    private static void SanitizeContent(IGuild guild, ReadOnlySpan<char> content, ref PlainTextMessageBuilder builder)
    {
        Utf8ValueStringBuilder wordBuffer = ZString.CreateUtf8StringBuilder();

        for (var index = 0; index < content.Length; index++)
        {
            char current = content[index];
            if (char.IsWhiteSpace(current))
            {
                AddWord(guild, ref builder, ref wordBuffer, current);
                wordBuffer.Clear();
            }
            else
            {
                wordBuffer.Append(current);
            }
        }

        if (wordBuffer.Length > 0)
        {
            AddWord(guild, ref builder, ref wordBuffer, '\0');
        }

        wordBuffer.Dispose();
    }

    private static void AddWord(IGuild guild,
        ref PlainTextMessageBuilder builder,
        ref Utf8ValueStringBuilder wordBuffer,
        char whitespaceTrivia)
    {
        using Utf8ValueStringBuilder buffer = ZString.CreateUtf8StringBuilder();

        ReadOnlySpan<byte> bytes = wordBuffer.AsSpan();
        int charCount = Utf8Encoding.GetCharCount(bytes);
        Span<char> chars = stackalloc char[charCount];
        Utf8Encoding.GetChars(bytes, chars);

        Span<char> temp = stackalloc char[255];

        var isEscaped = false;
        for (var index = 0; index < chars.Length; index++)
        {
            char current = chars[index];
            switch (current)
            {
                case '\\' when isEscaped:
                    buffer.Append('\\');
                    break;

                case '\\':
                    isEscaped = !isEscaped;
                    break;

                case '<':
                    index++;
                    int tagLength = ConsumeToEndOfTag(chars, ref index, temp);
                    char whitespace = index < chars.Length - 1 && char.IsWhiteSpace(chars[index]) ? chars[index] : '\0';
                    MentionUtility.ParseTag(guild, temp[..tagLength], ref builder, whitespace);
                    break;

                default:
                    buffer.Append(current);
                    break;
            }
        }

        bytes = buffer.AsSpan();
        charCount = Utf8Encoding.GetCharCount(bytes);
        chars = stackalloc char[charCount];
        Utf8Encoding.GetChars(bytes, chars);
        builder.AddWord(chars, whitespaceTrivia);
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

        if (string.IsNullOrWhiteSpace(message.Content) && message.Attachments.Count == 0)
        {
            author = null;
            message = null;
            return false;
        }

        return true;
    }

    private static int ConsumeToEndOfTag(ReadOnlySpan<char> word, ref int index, Span<char> element)
    {
        using Utf8ValueStringBuilder builder = ZString.CreateUtf8StringBuilder();
        var isEscaped = false;

        int startIndex = index;
        for (; index < word.Length; index++)
        {
            switch (word[index])
            {
                case '\\' when isEscaped:
                    builder.Append('\\');
                    isEscaped = false;
                    break;

                case '\\':
                    isEscaped = true;
                    break;

                case '>' when !isEscaped:
                    Utf8Encoding.GetChars(builder.AsSpan(), element);
                    return index + 1 - startIndex;

                default:
                    builder.Append(word[index]);
                    break;
            }
        }

        Utf8Encoding.GetChars(builder.AsSpan(), element);
        return index + 1 - startIndex;
    }
}
