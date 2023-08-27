using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VPLink.Commands;
using VPLink.Common.Configuration;
using VPLink.Common.Services;

namespace VPLink.Services;

internal sealed class DiscordService : BackgroundService
{
    private readonly ILogger<DiscordService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationService _configurationService;
    private readonly InteractionService _interactionService;
    private readonly DiscordSocketClient _discordClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configurationService">The configuration service.</param>
    /// <param name="interactionService">The interaction service.</param>
    /// <param name="discordClient">The Discord client.</param>
    public DiscordService(ILogger<DiscordService> logger,
        IServiceProvider serviceProvider,
        IConfigurationService configurationService,
        InteractionService interactionService,
        DiscordSocketClient discordClient)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configurationService = configurationService;
        _interactionService = interactionService;
        _discordClient = discordClient;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Establishing relay");

        _logger.LogInformation("Adding command modules");
        await _interactionService.AddModuleAsync<InfoCommand>(_serviceProvider).ConfigureAwait(false);
        await _interactionService.AddModuleAsync<WhoCommand>(_serviceProvider).ConfigureAwait(false);

        _discordClient.Ready += OnReady;
        _discordClient.InteractionCreated += OnInteractionCreated;

        IDiscordConfiguration configuration = _configurationService.DiscordConfiguration;
        string token = configuration.Token ?? throw new InvalidOperationException("Token is not set.");

        _logger.LogDebug("Connecting to Discord");
        await _discordClient.LoginAsync(TokenType.Bot, token);
        await _discordClient.StartAsync();
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
}
