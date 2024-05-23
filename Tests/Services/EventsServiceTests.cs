using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Tests.Services;

public class EventsServiceTests
{
    private readonly List<string> _defaultPartitionsIds;
    private readonly Properties _defaultProperties;
    private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

    public EventsServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.tests.json", false)
            .AddEnvironmentVariables().Build();

        _defaultPartitionsIds = new List<string> { "subtasking" };

        var defaultTaskOptions = new TaskOptions
        {
            MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
            MaxRetries = 2,
            Priority = 1,
            PartitionId = _defaultPartitionsIds[0]
        };

        _defaultProperties = new Properties(configuration, defaultTaskOptions, _defaultPartitionsIds);
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
            .Returns(() => Task.FromResult(false)); // End of stream

        streamReaderMock.SetupGet(x => x.Current)
            .Returns(() => responses.Dequeue());

        mockCallInvoker.Setup(invoker => invoker.AsyncServerStreamingCall(
                It.IsAny<Method<EventSubscriptionRequest, EventSubscriptionResponse>>(),
                It.IsAny<string>(),
                It.IsAny<CallOptions>(),
                It.IsAny<EventSubscriptionRequest>()))
            .Returns(new AsyncServerStreamingCall<EventSubscriptionResponse>
                (
                    streamReaderMock.Object,
                    Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(),
                    () => { }
                )
            );

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var eventsService =
            EventsServiceFactory.CreateEventsService(objectPool, new Session { Id = "1234" },
                NullLoggerFactory.Instance);
        // Act

        var blobId = new List<string> { "1234" };

        Assert.DoesNotThrowAsync(async () => await eventsService.WaitForBlobsAsync(blobId));
    }
}