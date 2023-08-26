using System.Reactive.Linq;
using System.Reactive.Subjects;
using NSubstitute;
using NSubstitute.Extensions;
using VPLink.Common.Data;
using VPLink.Common.Services;
using VpSharp.Extensions;

namespace VPLink.Tests;

public class RelayTests
{
    private readonly Subject<RelayedMessage> _vpMessageReceived = new();
    private readonly Subject<RelayedMessage> _discordMessageReceived = new();
    private IVirtualParadiseMessageService _vpMessageService = null!;
    private IDiscordMessageService _discordMessageService = null!;

    [SetUp]
    public void Setup()
    {
        _discordMessageService = Substitute.For<IDiscordMessageService>();
        _discordMessageService.Configure().SendMessageAsync(Arg.Any<RelayedMessage>()).Returns(Task.CompletedTask);

        _vpMessageService = Substitute.For<IVirtualParadiseMessageService>();
        _vpMessageService.Configure().OnMessageReceived.Returns(_vpMessageReceived.AsObservable());
        
        _discordMessageService = Substitute.For<IDiscordMessageService>();
        _discordMessageService.Configure().OnMessageReceived.Returns(_discordMessageReceived.AsObservable());
        
        _discordMessageReceived.SubscribeAsync(_vpMessageService.SendMessageAsync);
        _vpMessageReceived.SubscribeAsync(_discordMessageService.SendMessageAsync);
    }

    [Test]
    public void VirtualParadiseMessage_ShouldRelay_ToDiscordService()
    {
        var observer = Substitute.For<IObserver<RelayedMessage>>();
        _vpMessageReceived.Subscribe(observer);

        const string author = "Admin";
        const string message = "Hello, world!";

        _vpMessageReceived.OnNext(new RelayedMessage(author, message));

        observer.Received(1).OnNext(Arg.Is<RelayedMessage>(m => m.Author == author && m.Content == message));
        _discordMessageService.Received(1)
            .SendMessageAsync(Arg.Is<RelayedMessage>(m => m.Author == author && m.Content == message));
    }

    [Test]
    public void DiscordMessage_ShouldRelay_ToVirtualParadiseService()
    {
        var observer = Substitute.For<IObserver<RelayedMessage>>();
        _discordMessageReceived.Subscribe(observer);

        const string author = "Admin";
        const string message = "Hello, world!";

        _discordMessageReceived.OnNext(new RelayedMessage(author, message));

        observer.Received(1).OnNext(Arg.Is<RelayedMessage>(m => m.Author == author && m.Content == message));
        _vpMessageService.Received(1).SendMessageAsync(Arg.Is<RelayedMessage>(m => m.Author == author && m.Content == message));
    }
}
