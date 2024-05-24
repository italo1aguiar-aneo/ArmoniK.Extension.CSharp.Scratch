using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client;

public class ArmoniKClient
{
    private IBlobService _blobService;
    private IEventsService _eventsService;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Properties _properties;
    private ITasksService _tasksService;
    private ObjectPool<ChannelBase> _channelPool;
    private ISessionService _sessionService;

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
}