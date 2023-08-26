using System.Reactive.Linq;
using System.Reactive.Subjects;
using Discord;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Configuration;
using VpSharp;
using VpSharp.Entities;
using Color = System.Drawing.Color;
using VirtualParadiseConfiguration = VPLink.Configuration.VirtualParadiseConfiguration;

namespace VPLink.Services;

/// <inheritdoc cref="IVirtualParadiseService" />
internal sealed class VirtualParadiseService : BackgroundService, IVirtualParadiseService
{
    private readonly ILogger<VirtualParadiseService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly VirtualParadiseClient _virtualParadiseClient;
    private readonly Subject<VirtualParadiseMessage> _messageReceived = new();
    private readonly Subject<VirtualParadiseAvatar> _avatarJoined = new();
    private readonly Subject<VirtualParadiseAvatar> _avatarLeft = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualParadiseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public VirtualParadiseService(ILogger<VirtualParadiseService> logger,
        IConfigurationService configurationService,
        VirtualParadiseClient virtualParadiseClient)
    {
        _logger = logger;
        _configurationService = configurationService;
        _virtualParadiseClient = virtualParadiseClient;
    }

    /// <inheritdoc />
    public IObservable<VirtualParadiseAvatar> OnAvatarJoined => _avatarJoined.AsObservable();

    /// <inheritdoc />
    public IObservable<VirtualParadiseAvatar> OnAvatarLeft => _avatarJoined.AsObservable();

    /// <inheritdoc />
    public IObservable<VirtualParadiseMessage> OnMessageReceived => _messageReceived.AsObservable();

    /// <inheritdoc />
    public Task SendMessageAsync(IUserMessage message)
    {
        if (message is null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrWhiteSpace(message.Content)) return Task.CompletedTask;

        if (message.Author.IsBot && !_configurationService.BotConfiguration.RelayBotMessages)
        {
            _logger.LogDebug("Bot messages are disabled, ignoring message");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Message by {Author}: {Content}", message.Author, message.Content);

        string displayName = message.Author.GlobalName ?? message.Author.Username;
        return _virtualParadiseClient.SendMessageAsync(displayName, message.Content, FontStyle.Bold,
            Color.MidnightBlue);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");
        _virtualParadiseClient.MessageReceived.Subscribe(_messageReceived);
        _virtualParadiseClient.AvatarJoined.Subscribe(OnVirtualParadiseAvatarJoined);
        _virtualParadiseClient.AvatarLeft.Subscribe(OnVirtualParadiseAvatarLeft);

        VirtualParadiseConfiguration configuration = _configurationService.VirtualParadiseConfiguration;

        string username = configuration.Username ?? throw new InvalidOperationException("Username is not set.");
        string password = configuration.Password ?? throw new InvalidOperationException("Password is not set.");
        string world = configuration.World ?? throw new InvalidOperationException("World is not set.");
        string botName = configuration.BotName ?? throw new InvalidOperationException("Bot name is not set.");

        _logger.LogDebug("Connecting to Virtual Paradise");
        await _virtualParadiseClient.ConnectAsync().ConfigureAwait(false);
        await _virtualParadiseClient.LoginAsync(username, password, botName).ConfigureAwait(false);

        _logger.LogInformation("Entering world {World}", world);
        await _virtualParadiseClient.EnterAsync(world).ConfigureAwait(false);
    }

    private void OnVirtualParadiseAvatarJoined(VirtualParadiseAvatar avatar)
    {
        BotConfiguration configuration = _configurationService.BotConfiguration;

        if (!configuration.AnnounceAvatarEvents || avatar.IsBot && !configuration.AnnounceBots)
        {
            return;
        }

        _avatarJoined.OnNext(avatar);
    }

    private void OnVirtualParadiseAvatarLeft(VirtualParadiseAvatar avatar)
    {
        BotConfiguration configuration = _configurationService.BotConfiguration;

        if (!configuration.AnnounceAvatarEvents || avatar.IsBot && !configuration.AnnounceBots)
        {
            return;
        }

        _avatarLeft.OnNext(avatar);
    }
}
