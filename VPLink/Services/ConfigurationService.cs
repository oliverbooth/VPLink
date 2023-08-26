using Microsoft.Extensions.Configuration;
using VPLink.Common.Configuration;
using VPLink.Common.Services;
using VPLink.Configuration;

namespace VPLink.Services;

/// <inheritdoc cref="IConfigurationService" />
internal sealed class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationService" /> class.
    /// </summary>
    /// <param name="configuration"></param>
    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public IBotConfiguration BotConfiguration
    {
        get => _configuration.GetSection("Bot").Get<BotConfiguration>() ?? new BotConfiguration();
    }

    /// <inheritdoc />
    public IDiscordConfiguration DiscordConfiguration
    {
        get => _configuration.GetSection("Discord").Get<DiscordConfiguration>() ?? new DiscordConfiguration();
    }

    /// <inheritdoc />
    public IVirtualParadiseConfiguration VirtualParadiseConfiguration
    {
        get => _configuration.GetSection("VirtualParadise").Get<VirtualParadiseConfiguration>() ??
               new VirtualParadiseConfiguration();
    }
}
