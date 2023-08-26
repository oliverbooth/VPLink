using System.Reactive.Linq;
using System.Reactive.Subjects;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VpSharp;
using VpSharp.Entities;
using Color = System.Drawing.Color;

namespace VPLink.Services;

/// <inheritdoc cref="IVirtualParadiseService" />
internal sealed class VirtualParadiseService : BackgroundService, IVirtualParadiseService
{
    private readonly ILogger<VirtualParadiseService> _logger;
    private readonly IConfiguration _configuration;
    private readonly VirtualParadiseClient _virtualParadiseClient;
    private readonly Subject<VirtualParadiseMessage> _messageReceived = new();
    private readonly Subject<VirtualParadiseAvatar> _avatarJoined = new();
    private readonly Subject<VirtualParadiseAvatar> _avatarLeft = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualParadiseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="virtualParadiseClient">The Virtual Paradise client.</param>
    public VirtualParadiseService(ILogger<VirtualParadiseService> logger,
        IConfiguration configuration,
        VirtualParadiseClient virtualParadiseClient)
    {
        _logger = logger;
        _configuration = configuration;
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

        if (message.Author.IsBot && !_configuration.GetSection("Bot:RelayBotMessages").Get<bool>())
        {
            _logger.LogDebug("Bot messages are disabled, ignoring message");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Message by {Author}: {Content}", message.Author, message.Content);

        string displayName = message.Author.GlobalName ?? message.Author.Username;
        return _virtualParadiseClient.SendMessageAsync(displayName, message.Content, FontStyle.Bold, Color.MidnightBlue);
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");
        _virtualParadiseClient.MessageReceived.Subscribe(_messageReceived);
        _virtualParadiseClient.AvatarJoined.Subscribe(OnVirtualParadiseAvatarJoined);
        _virtualParadiseClient.AvatarLeft.Subscribe(OnVirtualParadiseAvatarLeft);

        string username = _configuration.GetSection("VirtualParadise:Username").Value ??
                          throw new InvalidOperationException("Username is not set.");
        string password = _configuration.GetSection("VirtualParadise:Password").Value ??
                          throw new InvalidOperationException("Password is not set.");
        string world = _configuration.GetSection("VirtualParadise:World").Value ??
                       throw new InvalidOperationException("World is not set.");
        string botName = _configuration.GetSection("VirtualParadise:BotName").Value ??
                         throw new InvalidOperationException("Bot name is not set.");

        _logger.LogDebug("Connecting to Virtual Paradise");
        await _virtualParadiseClient.ConnectAsync().ConfigureAwait(false);
        await _virtualParadiseClient.LoginAsync(username, password, botName).ConfigureAwait(false);

        _logger.LogInformation("Entering world {World}", world);
        await _virtualParadiseClient.EnterAsync(world).ConfigureAwait(false);
    }

    private void OnVirtualParadiseAvatarJoined(VirtualParadiseAvatar avatar)
    {
        if (!_configuration.GetValue<bool>("Bot:AnnounceAvatarEvents"))
        {
            _logger.LogDebug("Join/leave events are disabled, ignoring event");
            return;
        }

        if (avatar.IsBot && !_configuration.GetSection("Bot:AnnounceBots").Get<bool>())
        {
            _logger.LogDebug("Bot events are disabled, ignoring event");
            return;
        }

        _avatarJoined.OnNext(avatar);
    }

    private void OnVirtualParadiseAvatarLeft(VirtualParadiseAvatar avatar)
    {
        if (!_configuration.GetValue<bool>("Bot:AnnounceAvatarEvents"))
        {
            _logger.LogDebug("Join/leave events are disabled, ignoring event");
            return;
        }

        if (avatar.IsBot && !_configuration.GetSection("Bot:AnnounceBots").Get<bool>())
        {
            _logger.LogDebug("Bot events are disabled, ignoring event");
            return;
        }

        _avatarLeft.OnNext(avatar);
    }
}
