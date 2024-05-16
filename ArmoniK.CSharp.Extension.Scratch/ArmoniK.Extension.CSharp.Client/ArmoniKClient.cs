using ArmoniK.Api.gRPC.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArmoniK.Utils;
using Grpc.Core;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Extension.CSharp.Client.Services;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client;

public class ArmoniKClient
{
    private IBlobService _blobService;
    private ITasksService _tasksService;
    private ISessionService _sessionService;
    private IEventsService _eventsService;
    private ObjectPool<ChannelBase> _channelPool;
    private readonly Properties _properties;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    public ArmoniKClient(Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ArmoniKClient>();
    }

    public async Task LaunchServices()
    {
        ChannelBase channel = await ChannelPool.GetAsync();
        _blobService = BlobServiceFactory.CreateBlobService(channel, _loggerFactory);
        _sessionService = SessionServiceFactory.CreateSessionService(channel, _properties, _loggerFactory);
        _tasksService = TasksServiceFactory.CreateTaskService(channel, _loggerFactory);
        _eventsService = EventsServiceFactory.CreateEventsService(channel, _loggerFactory);
    }

    public ObjectPool<ChannelBase> ChannelPool
        => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
            _loggerFactory);

    public async Task<IBlobService> GetBlobService()
    {
        if (_blobService is not null) return _blobService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _blobService = BlobServiceFactory.CreateBlobService(channel, _loggerFactory);
        return _blobService;
    }

    public async Task<ISessionService> GetSessionService()
    {
        if (_sessionService is not null) return _sessionService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _sessionService = SessionServiceFactory.CreateSessionService(channel, _properties, _loggerFactory);
        return _sessionService;
    }

    public async Task<ITasksService> GetTasksService()
    {
        if (_tasksService is not null) return _tasksService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _tasksService = TasksServiceFactory.CreateTaskService(channel, _loggerFactory);
        return _tasksService;
    }

    public async Task<IEventsService> GetEventsService()
    {
        if (_eventsService is not null) return _eventsService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _eventsService = EventsServiceFactory.CreateEventsService(channel, _loggerFactory);
        return _eventsService;
    }
}