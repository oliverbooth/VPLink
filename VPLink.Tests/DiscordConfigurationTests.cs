using NSubstitute;
using NSubstitute.Extensions;
using VPLink.Common.Configuration;
using VPLink.Common.Services;

namespace VPLink.Tests;

public class DiscordConfigurationTests
{
    private IConfigurationService _configurationService = null!;

    [SetUp]
    public void Setup()
    {
        var configuration = Substitute.For<IDiscordConfiguration>();
        configuration.Token.Returns("DISCORD_TOKEN");
        configuration.ChannelId.Returns(1234567890UL);

        _configurationService = Substitute.For<IConfigurationService>();
        _configurationService.Configure().DiscordConfiguration.Returns(configuration);
    }

    [Test]
    public void DiscordConfiguration_ShouldReturnCorrectValues_GivenDefaultConfig()
    {
        IDiscordConfiguration configuration = _configurationService.DiscordConfiguration;
        Assert.Multiple(() =>
        {
            Assert.That(configuration.Token, Is.EqualTo("DISCORD_TOKEN"));
            Assert.That(configuration.ChannelId, Is.EqualTo(1234567890UL));
        });
    }
}
