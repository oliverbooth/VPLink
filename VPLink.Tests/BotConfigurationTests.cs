using NSubstitute;
using NSubstitute.Extensions;
using VPLink.Common.Configuration;
using VPLink.Common.Services;

namespace VPLink.Tests;

public class BotConfigurationTests
{
    private IConfigurationService _configurationService = null!;

    [SetUp]
    public void Setup()
    {
        var configuration = Substitute.For<IBotConfiguration>();
        configuration.AnnounceAvatarEvents.Returns(true);
        configuration.AnnounceBots.Returns(false);
        configuration.RelayBotMessages.Returns(false);

        _configurationService = Substitute.For<IConfigurationService>();
        _configurationService.Configure().BotConfiguration.Returns(configuration);
    }

    [Test]
    public void BotConfiguration_ShouldReturnCorrectValues_GivenDefaultConfig()
    {
        IBotConfiguration configuration = _configurationService.BotConfiguration;
        Assert.Multiple(() =>
        {
            Assert.That(configuration.AnnounceAvatarEvents, Is.True);
            Assert.That(configuration.AnnounceBots, Is.False);
            Assert.That(configuration.RelayBotMessages, Is.False);
        });
    }
}
