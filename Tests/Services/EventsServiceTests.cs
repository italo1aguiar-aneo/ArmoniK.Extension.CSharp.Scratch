using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Tests.Services;

public class EventsServiceTests
{
    private readonly List<string> _defaultPartitionsIds;
    private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

    public EventsServiceTests()
    {
        _defaultPartitionsIds = new List<string> { "subtasking" };
    }

    [Test]
    public async Task CreateSession_ReturnsNewSessionWithId()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        var responses = new Queue<EventSubscriptionResponse>(
            new[]
            {
                new EventSubscriptionResponse
                {
                    SessionId = "1234", NewResult = new EventSubscriptionResponse.Types.NewResult
                    {
                        ResultId = "1234",
                        Status = ResultStatus.Completed
                    }
                }
            }
        );

        // Setup the mock stream reader
        var streamReaderMock = new Mock<IAsyncStreamReader<EventSubscriptionResponse>>();
        streamReaderMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(responses.Count > 0))
            .Returns(() => Task.FromResult(false));

        streamReaderMock.SetupGet(x => x.Current)
            .Returns(() => responses.Dequeue());

        mockCallInvoker.Setup(invoker => invoker.AsyncServerStreamingCall(
                    It.IsAny<Method<EventSubscriptionRequest, EventSubscriptionResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<EventSubscriptionRequest>()
                )
            )
            .Returns(new AsyncServerStreamingCall<EventSubscriptionResponse>
                (
                    streamReaderMock.Object,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                )
            );

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var eventsService =
            EventsServiceFactory.CreateEventsService(objectPool, NullLoggerFactory.Instance);
        // Act

        var blobId = new List<string> { "1234" };

        Assert.DoesNotThrowAsync(async () => await eventsService.WaitForBlobsAsync(new SessionInfo("sessionId"), blobId));
    }
}