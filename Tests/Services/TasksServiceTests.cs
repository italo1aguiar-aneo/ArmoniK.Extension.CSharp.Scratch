using System.Collections.Immutable;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests.Services;

public class TasksServiceTests
{
    private readonly List<string> _defaultPartitionsIds;
    private readonly Properties _defaultProperties;
    private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

    public TasksServiceTests()
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
    public async Task CreateTask_ReturnsNewTaskWithId()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        var mockCallInvoker = new Mock<CallInvoker>();

        mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(
            It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
            It.IsAny<string>(),
            It.IsAny<CallOptions>(),
            It.IsAny<SubmitTasksRequest>()
        )).Returns(
            new AsyncUnaryCall<SubmitTasksResponse>(
                Task.FromResult(new SubmitTasksResponse
                    {
                        TaskInfos =
                        {
                            new SubmitTasksResponse.Types.TaskInfo
                            {
                                TaskId = "taskId1",
                                ExpectedOutputIds = { new List<string> { "blobId1" } },
                                PayloadId = "payloadId1"
                            }
                        }
                    }
                ),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { })
        );
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var taskNodes = new List<TaskNode>
        {
            new()
            {
                ExpectedOutputs = new List<BlobInfo>
                {
                    new("blob1", "blobId1", new Session { Id = "sessionId1" })
                },
                Payload = new BlobInfo("payload1", "payloadId1", new Session { Id = "sessionId1" })
            }
        };

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var mockBlobService = new Mock<IBlobService>().Object;

        var taskService =
            TasksServiceFactory.CreateTaskService(objectPool, mockBlobService, new Session { Id = "sessionId1" },
                NullLoggerFactory.Instance);
        // Act
        var result = await taskService.SubmitTasksAsync(taskNodes);

        // Assert
        ClassicAssert.AreEqual("taskId1", result.FirstOrDefault()?.TaskId);
        ClassicAssert.AreEqual("payloadId1", result.FirstOrDefault()?.PayloadId);
        ClassicAssert.AreEqual("blobId1", result.FirstOrDefault()?.ExpectedOutputIds[0]);
    }

    [Test]
    public async Task SubmitTasksAsync_MultipleTasksWithOutputs_ReturnsCorrectResponses()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        var mockCallInvoker = new Mock<CallInvoker>();
        var taskResponse = new SubmitTasksResponse
        {
            TaskInfos =
            {
                new SubmitTasksResponse.Types.TaskInfo
                    { TaskId = "taskId1", PayloadId = "payloadId1", ExpectedOutputIds = { "outputId1" } },
                new SubmitTasksResponse.Types.TaskInfo
                    { TaskId = "taskId2", PayloadId = "payloadId2", ExpectedOutputIds = { "outputId2" } }
            }
        };

        mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(
            It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
            It.IsAny<string>(),
            It.IsAny<CallOptions>(),
            It.IsAny<SubmitTasksRequest>()
        )).Returns(new AsyncUnaryCall<SubmitTasksResponse>(
            Task.FromResult(taskResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { })
        );

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        var mockBlobService = new Mock<IBlobService>().Object;
        var taskService = TasksServiceFactory.CreateTaskService(objectPool, mockBlobService,
            new Session { Id = "sessionId1" }, NullLoggerFactory.Instance);

        var taskNodes = new List<TaskNode>
        {
            new()
            {
                Payload = new BlobInfo("payload1", "payloadId1", new Session { Id = "sessionId1" }),
                ExpectedOutputs =
                    new List<BlobInfo> { new("output1", "outputId1", new Session { Id = "sessionId1" }) }
            },
            new()
            {
                Payload = new BlobInfo("payload2", "payloadId2", new Session { Id = "sessionId1" }),
                ExpectedOutputs = new List<BlobInfo> { new("output2", "outputId2", new Session { Id = "sessionId2" }) }
            }
        };

        // Act
        var result = await taskService.SubmitTasksAsync(taskNodes);

        // Assert
        ClassicAssert.AreEqual(2, result.Count());
        Assert.That(result,
            Has.Some.Matches<SubmitTasksResponse.Types.TaskInfo>(r =>
                r.TaskId == "taskId1" && r.PayloadId == "payloadId1" && r.ExpectedOutputIds.Contains("outputId1")),
            "Result should contain an item with taskId1, payloadId1, and outputId1");

        Assert.That(result,
            Has.Some.Matches<SubmitTasksResponse.Types.TaskInfo>(r =>
                r.TaskId == "taskId2" && r.PayloadId == "payloadId2" && r.ExpectedOutputIds.Contains("outputId2")),
            "Result should contain an item with taskId2, payloadId2, and outputId2");
    }


    [Test]
    public async Task SubmitTasksAsync_WithEmptyExpectedOutputs_ThrowsException()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        var mockCallInvoker = new Mock<CallInvoker>();
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        var mockBlobService = new Mock<IBlobService>().Object;
        var taskService = TasksServiceFactory.CreateTaskService(objectPool, mockBlobService,
            new Session { Id = "sessionId1" }, NullLoggerFactory.Instance);

        var taskNodes = new List<TaskNode>
        {
            new()
            {
                Payload = new BlobInfo("payload1", "payloadId1", new Session { Id = "sessionId1" }),
                ExpectedOutputs = new List<BlobInfo>() // Empty expected outputs
            }
        };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => taskService.SubmitTasksAsync(taskNodes));
    }

    [Test]
    public async Task SubmitTasksAsync_WithDataDependencies_CreatesBlobsCorrectly()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        var mockCallInvoker = new Mock<CallInvoker>();
        var taskResponse = new SubmitTasksResponse
        {
            TaskInfos =
            {
                new SubmitTasksResponse.Types.TaskInfo
                {
                    TaskId = "taskId1", PayloadId = "payloadId1", ExpectedOutputIds = { "outputId1" },
                    DataDependencies = { "dependencyBlobId" }
                }
            }
        };

        mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(
            It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
            It.IsAny<string>(),
            It.IsAny<CallOptions>(),
            It.IsAny<SubmitTasksRequest>()
        )).Returns(new AsyncUnaryCall<SubmitTasksResponse>(
            Task.FromResult(taskResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { })
        );

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        var mockBlobService = new Mock<IBlobService>();

        var expectedBlobs = new List<BlobInfo>
        {
            new("dependencyBlob", "dependencyBlobId", new Session { Id = "sessionId1" })
        };

        mockBlobService.Setup(m =>
                m.CreateBlobsAsync(It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBlobs);

        var taskService = TasksServiceFactory.CreateTaskService(objectPool, mockBlobService.Object,
            new Session { Id = "sessionId1" }, NullLoggerFactory.Instance);

        var dataDependenciesContent = new Dictionary<string, ReadOnlyMemory<byte>>
        {
            { "dependencyBlob", new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }) }
        };

        var taskNodes = new List<TaskNode>
        {
            new()
            {
                Payload = new BlobInfo("payloadId", "blobId", new Session { Id = "sessionId1" }),
                ExpectedOutputs = new List<BlobInfo> { new("output1", "outputId1", new Session { Id = "sessionId1" }) },
                DataDependenciesContent = dataDependenciesContent
            }
        };

        // Act
        var result = await taskService.SubmitTasksAsync(taskNodes);

        // Assert
        mockBlobService.Verify(
            m => m.CreateBlobsAsync(It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        ClassicAssert.AreEqual("dependencyBlobId", taskNodes.First().DataDependencies.First().Id);
        ClassicAssert.AreEqual("dependencyBlobId", result.First().DataDependencies.First());
    }

    [Test]
    public async Task SubmitTasksAsync_EmptyDataDependencies_DoesNotCreateBlobs()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        var mockCallInvoker = new Mock<CallInvoker>();
        var taskResponse = new SubmitTasksResponse
        {
            TaskInfos =
            {
                new SubmitTasksResponse.Types.TaskInfo
                    { TaskId = "taskId1", PayloadId = "payloadId1", ExpectedOutputIds = { "outputId1" } }
            }
        };

        mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(
            It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
            It.IsAny<string>(),
            It.IsAny<CallOptions>(),
            It.IsAny<SubmitTasksRequest>()
        )).Returns(new AsyncUnaryCall<SubmitTasksResponse>(
            Task.FromResult(taskResponse),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { })
        );

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        var mockBlobService = new Mock<IBlobService>();

        var expectedBlobs = new List<BlobInfo>
        {
            new("dependencyBlob", "dependencyBlobId", new Session { Id = "sessionId1" })
        };

        var taskService = TasksServiceFactory.CreateTaskService(objectPool, mockBlobService.Object,
            new Session { Id = "sessionId1" }, NullLoggerFactory.Instance);

        var dataDependenciesContent = ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

        var taskNodes = new List<TaskNode>
        {
            new()
            {
                Payload = new BlobInfo("payloadId", "blobId", new Session { Id = "sessionId1" }),
                ExpectedOutputs = new List<BlobInfo> { new("output1", "outputId1", new Session { Id = "sessionId1" }) },
                DataDependenciesContent = dataDependenciesContent
            }
        };

        // Act
        var result = await taskService.SubmitTasksAsync(taskNodes);

        // Assert
        mockBlobService.Verify(
            m => m.CreateBlobsAsync(It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        Assert.That(taskNodes.First().DataDependencies, Is.Empty);
    }
}