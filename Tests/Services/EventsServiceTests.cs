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
using Xunit;

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

    [Fact]
    public async Task CreateSession_ReturnsNewSessionWithId()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        mockCallInvoker.Setup(invoker => invoker.AsyncServerStreamingCall(
                It.IsAny<Method<EventSubscriptionRequest, EventSubscriptionResponse>>(),
                It.IsAny<string>(),
                It.IsAny<CallOptions>(),
                It.IsAny<EventSubscriptionRequest>()))
            .Returns(new AsyncServerStreamingCall<EventSubscriptionResponse>
                (Task.FromResult(new EventSubscriptionResponse
                    {
                        NewResult = new EventSubscriptionResponse.Types.NewResult
                        {
                            OwnerId = "1234",
                            ResultId = "1234",
                            Status = ResultStatus.Completed
                        }
                    }) as IAsyncStreamReader<EventSubscriptionResponse>,
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

        await eventsService.WaitForBlobsAsync(blobId);
    }
}