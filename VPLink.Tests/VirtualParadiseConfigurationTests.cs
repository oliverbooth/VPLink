using NSubstitute;
using NSubstitute.Extensions;
using VPLink.Common.Configuration;
using VPLink.Common.Services;

namespace VPLink.Tests;

public class VirtualParadiseConfigurationTests
{
    private IConfigurationService _configurationService = null!;

    [SetUp]
    public void Setup()
    {
        var configuration = Substitute.For<IVirtualParadiseConfiguration>();
        configuration.Username.Returns("Admin");
        configuration.Password.Returns("Password1234");
        configuration.World.Returns("Blizzard");
        configuration.BotName.Returns("VPLink");

        _configurationService = Substitute.For<IConfigurationService>();
        _configurationService.Configure().VirtualParadiseConfiguration.Returns(configuration);
    }

    [Test]
    public void DiscordConfiguration_ShouldReturnCorrectValues_GivenDefaultConfig()
    {
        IVirtualParadiseConfiguration configuration = _configurationService.VirtualParadiseConfiguration;
        Assert.Multiple(() =>
        {
            Assert.That(configuration.Username, Is.EqualTo("Admin"));
            Assert.That(configuration.Password, Is.EqualTo("Password1234"));
            Assert.That(configuration.World, Is.EqualTo("Blizzard"));
            Assert.That(configuration.BotName, Is.EqualTo("VPLink"));
        });
    }
}
