using System.Reactive.Subjects;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Common.Configuration;
using VPLink.Common.Services;
using VpSharp;
using VpSharp.Entities;

namespace VPLink.Services;

internal sealed class VirtualParadiseService : BackgroundService
{
    private readonly ILogger<VirtualParadiseService> _logger;
    private readonly IConfigurationService _configurationService;
    private readonly VirtualParadiseClient _virtualParadiseClient;
    private readonly Subject<VirtualParadiseMessage> _messageReceived = new();

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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");
        _virtualParadiseClient.MessageReceived.Subscribe(_messageReceived);

        IVirtualParadiseConfiguration configuration = _configurationService.VirtualParadiseConfiguration;

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
}
