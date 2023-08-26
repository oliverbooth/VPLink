using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Commands;
using VpSharp;
using VpSharp.Entities;

namespace VPLink.Services;

/// <inheritdoc cref="IDiscordService" />
internal sealed partial class DiscordService : BackgroundService, IDiscordService
{
    private static readonly Regex UnescapeRegex = GetUnescapeRegex();
    private static readonly Regex EscapeRegex = GetEscapeRegex();

    private readonly ILogger<DiscordService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _discordClient;
    private readonly VirtualParadiseClient _virtualParadiseClient;
    private readonly Subject<IUserMessage> _messageReceived = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="interactionService">The interaction service.</param>
    /// <param name="discordClient">The Discord client.</param>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public DiscordService(ILogger<DiscordService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        InteractionService interactionService,
        DiscordSocketClient discordClient,
        VirtualParadiseClient virtualParadiseClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _interactionService = interactionService;
        _discordClient = discordClient;
        _virtualParadiseClient = virtualParadiseClient;
    }

    /// <inheritdoc />
    public IObservable<IUserMessage> OnMessageReceived => _messageReceived.AsObservable();

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");

        _logger.LogInformation("Adding command modules");
        await _interactionService.AddModuleAsync<WhoCommand>(_serviceProvider).ConfigureAwait(false);

        _discordClient.Ready += OnReady;
        _discordClient.InteractionCreated += OnInteractionCreated;
        _discordClient.MessageReceived += OnDiscordMessageReceived;

        string token = _configuration.GetSection("Discord:Token").Value ??
                       throw new InvalidOperationException("Token is not set.");

        _logger.LogDebug("Connecting to Discord");
        await _discordClient.LoginAsync(TokenType.Bot, token);
        await _discordClient.StartAsync();
    }

    private Task OnDiscordMessageReceived(SocketMessage arg)
    {
        if (arg is not IUserMessage message)
            return Task.CompletedTask;

        if (message.Channel.Id != _configuration.GetSection("Discord:ChannelId").Get<ulong>())
            return Task.CompletedTask;

        _messageReceived.OnNext(message);
        return Task.CompletedTask;
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_discordClient, interaction);
            IResult result = await _interactionService.ExecuteCommandAsync(context, _serviceProvider);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                }
        }
        catch
        {
            if (interaction.Type is InteractionType.ApplicationCommand)
                await interaction.GetOriginalResponseAsync().ContinueWith(async msg => await msg.Result.DeleteAsync());
        }
    }

    private Task OnReady()
    {
        _logger.LogInformation("Discord client ready");
        return _interactionService.RegisterCommandsGloballyAsync();
    }

    /// <inheritdoc />
    public Task SendMessageAsync(VirtualParadiseMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrWhiteSpace(message.Content)) return Task.CompletedTask;

        if (message.Author is not { } author)
        {
            _logger.LogWarning("Received message without author, ignoring message");
            return Task.CompletedTask;
        }

        if (author.IsBot && !_configuration.GetSection("Bot:RelayBotMessages").Get<bool>())
        {
            _logger.LogDebug("Bot messages are disabled, ignoring message");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Message by {Author}: {Content}", author, message.Content);

        var channelId = _configuration.GetSection("Discord:ChannelId").Get<ulong>();
        if (_discordClient.GetChannel(channelId) is not ITextChannel channel)
        {
            _logger.LogError("Channel {ChannelId} does not exist", channelId);
            return Task.CompletedTask;
        }

        string unescaped = UnescapeRegex.Replace(message.Content, "$1");
        string escaped = EscapeRegex.Replace(unescaped, "\\$1");

        string displayName = author.Name;
        return channel.SendMessageAsync($"**{displayName}**: {escaped}");
    }

    [GeneratedRegex(@"\\(\*|_|`|~|\\)", RegexOptions.Compiled)]
    private static partial Regex GetUnescapeRegex();

    [GeneratedRegex(@"(\*|_|`|~|\\)", RegexOptions.Compiled)]
    private static partial Regex GetEscapeRegex();
}
