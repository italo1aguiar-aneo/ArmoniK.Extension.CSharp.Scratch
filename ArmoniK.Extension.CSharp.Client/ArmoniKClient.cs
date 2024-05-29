using System;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Extension.CSharp.Client.Handlers;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client;

public class ArmoniKClient
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Properties _properties;
    private IBlobService _blobService;
    private ObjectPool<ChannelBase> _channelPool;
    private IEventsService _eventsService;
    private ISessionService _sessionService;
    private ITasksService _tasksService;

    public ArmoniKClient(Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<ArmoniKClient>();
    }

    public ObjectPool<ChannelBase> ChannelPool
        => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
            _loggerFactory);

    public Task<IBlobService> GetBlobService()
    {
        if (_blobService is not null) return Task.FromResult(_blobService);
        _blobService = BlobServiceFactory.CreateBlobService(ChannelPool, _loggerFactory);
        return Task.FromResult(_blobService);
    }

    public Task<ISessionService> GetSessionService()
    {
        if (_sessionService is not null) return Task.FromResult(_sessionService);
        _sessionService = SessionServiceFactory.CreateSessionService(ChannelPool, _properties, _loggerFactory);
        return Task.FromResult(_sessionService);
    }

    public async Task<ITasksService> GetTasksService()
    {
        if (_tasksService is not null) return _tasksService;
        _tasksService =
            TasksServiceFactory.CreateTaskService(ChannelPool, await GetBlobService(), _loggerFactory);
        return _tasksService;
    }

    public Task<IEventsService> GetEventsService()
    {
        if (_eventsService is not null)
            return Task.FromResult(_eventsService);
        _eventsService =
            EventsServiceFactory.CreateEventsService(ChannelPool, _loggerFactory);
        return Task.FromResult(_eventsService);
    }

    public Task<BlobHandler> GetBlobHandler(BlobInfo blobInfo)
    {
        return Task.FromResult(new BlobHandler(blobInfo, this));
    }

    public Task<TaskHandler> GetTaskHandler(TaskInfos taskInfos)
    {
        return Task.FromResult(new TaskHandler(this, taskInfos));
    }

    public Task<SessionHandler> GetSessionHandler(SessionInfo session)
    {
        return Task.FromResult(new SessionHandler(session, this));
    }
}