using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace Tests.Services;

public class BlobServiceTests
{
    private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

    [Fact]
    public async Task CreateBlob_ReturnsNewBlobInfo()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsMetaDataRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(
                Task.FromResult(new CreateResultsMetaDataResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = "blobName", ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));


        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var blobService =
            BlobServiceFactory.CreateBlobService(objectPool, new Session { Id = "sessionId" },
                NullLoggerFactory.Instance);
        // Act
        var result = await blobService.CreateBlobAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessionId", result.Session.Id);
    }

    [Fact]
    public async Task CreateBlob_WithName_ReturnsNewBlobInfo()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        var name = "blobName";

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsMetaDataRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(
                Task.FromResult(new CreateResultsMetaDataResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));


        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var blobService =
            BlobServiceFactory.CreateBlobService(objectPool, new Session { Id = "sessionId" },
                NullLoggerFactory.Instance);

        // Act
        var result = await blobService.CreateBlobAsync(name);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessionId", result.Session.Id);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public async Task CreateBlobAsync_WithIAsyncEnumerableContent_CreatesBlobAndUploadsContent()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        var name = "blobName";
        var contents = AsyncEnumerable.Range(1, 100).Select(x => new ReadOnlyMemory<byte>(new[] { (byte)x }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<Empty>()
                )
            )
            .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(
                Task.FromResult(new ResultsServiceConfigurationResponse
                {
                    DataChunkMaxSize = 20
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsMetaDataRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(
                Task.FromResult(new CreateResultsMetaDataResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

        var responseTask = Task.FromResult(new UploadResultDataResponse
        {
            Result = new ResultRaw { Name = "anyResult", ResultId = "anyResultId" }
        });

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncClientStreamingCall(
                    It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>()
                ))
            .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(
                mockStream.Object,
                responseTask,
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
            ));

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var blobService =
            BlobServiceFactory.CreateBlobService(objectPool, new Session { Id = "sessionId" },
                NullLoggerFactory.Instance);

        // Act
        var result = await blobService.CreateBlobAsync(name, contents);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessionId", result.Session.Id);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public async Task CreateBlobAsync_WithContent_CreatesBlobAndUploadsContent()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        var name = "blobName";
        var contents = new ReadOnlyMemory<byte>(Enumerable.Range(1, 20).Select(x => (byte)x).ToArray());

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<Empty>()
                )
            )
            .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(
                Task.FromResult(new ResultsServiceConfigurationResponse
                {
                    DataChunkMaxSize = 500
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsMetaDataRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(
                Task.FromResult(new CreateResultsMetaDataResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsRequest, CreateResultsResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsResponse>(
                Task.FromResult(new CreateResultsResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));


        var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

        var responseTask = Task.FromResult(new UploadResultDataResponse
        {
            Result = new ResultRaw { Name = "anyResult", ResultId = "anyResultId" }
        });

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncClientStreamingCall(
                    It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>()
                ))
            .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(
                mockStream.Object,
                responseTask,
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
            ));

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var blobService =
            BlobServiceFactory.CreateBlobService(objectPool, new Session { Id = "sessionId" },
                NullLoggerFactory.Instance);

        // Act
        var result = await blobService.CreateBlobAsync(name, contents);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessionId", result.Session.Id);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public async Task CreateBlobAsync_WithBigContent_CreatesBlobAndUploadsContent()
    {
        // Arrange
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };

        // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
        var mockCallInvoker = new Mock<CallInvoker>();

        var name = "blobName";
        var contents = new ReadOnlyMemory<byte>(Enumerable.Range(1, 100).Select(x => (byte)x).ToArray());

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<Empty>()
                )
            )
            .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(
                Task.FromResult(new ResultsServiceConfigurationResponse
                {
                    DataChunkMaxSize = 20
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsMetaDataRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(
                Task.FromResult(new CreateResultsMetaDataResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncUnaryCall(
                    It.IsAny<Method<CreateResultsRequest, CreateResultsResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<CreateResultsRequest>()))
            .Returns(new AsyncUnaryCall<CreateResultsResponse>(
                Task.FromResult(new CreateResultsResponse
                {
                    Results =
                    {
                        new ResultRaw
                        {
                            CompletedAt = DateTime.Now.ToUniversalTime().ToTimestamp(), Status = ResultStatus.Created,
                            Name = name, ResultId = "blodId", SessionId = "sessionId"
                        }
                    }
                }),
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }));


        var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

        var responseTask = Task.FromResult(new UploadResultDataResponse
        {
            Result = new ResultRaw { Name = "anyResult", ResultId = "anyResultId" }
        });

        mockCallInvoker.Setup(invoker =>
                invoker.AsyncClientStreamingCall(
                    It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>()
                ))
            .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(
                mockStream.Object,
                responseTask,
                Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { }
            ));

        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockCallInvoker.Object);

        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

        var blobService =
            BlobServiceFactory.CreateBlobService(objectPool, new Session { Id = "sessionId" },
                NullLoggerFactory.Instance);

        // Act
        var result = await blobService.CreateBlobAsync(name, contents);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sessionId", result.Session.Id);
        Assert.Equal(name, result.Name);
    }
}