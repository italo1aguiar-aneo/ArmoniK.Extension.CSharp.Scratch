using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.Helpers;

internal static class MockHelper
{
    public static Mock<CallInvoker> SetupAsyncUnaryCallInvokerMock<TReq, TRes>(this Mock<CallInvoker> mockInvoker,
        TRes returnData) where TReq : class where TRes : class
    {
        var responseTask = Task.FromResult(returnData);
        var responseHeadersTask = Task.FromResult(new Metadata());

        mockInvoker.Setup(invoker => invoker.AsyncUnaryCall(
                It.IsAny<Method<TReq, TRes>>(),
                It.IsAny<string>(),
                It.IsAny<CallOptions>(),
                It.IsAny<TReq>()
            )
        ).Returns(
            new AsyncUnaryCall<TRes>(
                responseTask,
                responseHeadersTask,
                StatusFunc,
                TrailersFunc,
                DisposeAction
            )
        );

        return mockInvoker;

        void DisposeAction()
        {
        }

        Metadata TrailersFunc()
        {
            return []
            ;
        }

        Status StatusFunc()
        {
            return Status.DefaultSuccess;
        }
    }

    public static Mock<CallInvoker> SetupAsyncClientStreamingCall<TReq, TRes>(this Mock<CallInvoker> mockInvoker,
        TRes returnData, IClientStreamWriter<TReq> stream)
        where TReq : class where TRes : class
    {
        var responseTask = Task.FromResult(returnData);
        mockInvoker.Setup(invoker =>
                invoker.AsyncClientStreamingCall(
                    It.IsAny<Method<TReq, TRes>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>()
                )
            )
            .Returns(
                new AsyncClientStreamingCall<TReq, TRes>(
                    stream,
                    responseTask,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                )
            );
        return mockInvoker;
    }

    public static Mock<CallInvoker> SetupAsyncServerStreamingCallInvokerMock<TReq, TRes>(
        this Mock<CallInvoker> mockInvoker,
        TRes returnData) where TReq : class where TRes : class
    {
        var streamReaderMock = new Mock<IAsyncStreamReader<TRes>>();

        streamReaderMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(true))
            .Returns(() => Task.FromResult(false));

        streamReaderMock.SetupGet(x => x.Current)
            .Returns(() => returnData);

        mockInvoker.Setup(invoker => invoker.AsyncServerStreamingCall(
                    It.IsAny<Method<TReq, TRes>>(),
                    It.IsAny<string>(),
                    It.IsAny<CallOptions>(),
                    It.IsAny<TReq>()
                )
            )
            .Returns(new AsyncServerStreamingCall<TRes>
                (
                    streamReaderMock.Object,
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                )
            );

        return mockInvoker;
    }


    public static ITasksService GetTasksServiceMock(Mock<CallInvoker>? mockInvoker, Mock<IBlobService>? mockBlobService)
    {
        mockInvoker ??= new Mock<CallInvoker>();
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        mockBlobService ??= new Mock<IBlobService>();

        var taskService =
            TasksServiceFactory.CreateTaskService(objectPool, mockBlobService.Object, NullLoggerFactory.Instance);
        return taskService;
    }

    public static IEventsService GetEventsServiceMock(Mock<CallInvoker>? mockInvoker)
    {
        mockInvoker ??= new Mock<CallInvoker>();
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        return EventsServiceFactory.CreateEventsService(objectPool, NullLoggerFactory.Instance);
    }

    public static ISessionService GetSessionServiceMock(Mock<CallInvoker>? mockInvoker, Properties properties)
    {
        mockInvoker ??= new Mock<CallInvoker>();
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        return SessionServiceFactory.CreateSessionService(objectPool, properties, NullLoggerFactory.Instance);
    }

    public static IBlobService GetBlobServiceMock(Mock<CallInvoker>? mockInvoker)
    {
        mockInvoker ??= new Mock<CallInvoker>();
        var mockChannelBase = new Mock<ChannelBase>("localhost") { CallBase = true };
        mockChannelBase.Setup(m => m.CreateCallInvoker()).Returns(mockInvoker.Object);
        var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
        return BlobServiceFactory.CreateBlobService(objectPool, NullLoggerFactory.Instance);
    }

    public static Mock<IBlobService> SetupCreateBlobMock(this Mock<IBlobService> blobService, List<BlobInfo> returnData)
    {
        blobService.Setup(m =>
                m.CreateBlobsAsync(It.IsAny<SessionInfo>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                    It.IsAny<CancellationToken>()))
            .Returns(returnData.ToAsyncEnumerable);

        return blobService;
    }
}