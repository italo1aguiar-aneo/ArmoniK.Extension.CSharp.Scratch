using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client;

public class ArmoniKClient
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Properties _properties;
    private readonly Dictionary<Session,IBlobService> _blobServiceDictionary = new();
    private ObjectPool<ChannelBase> _channelPool;
    private readonly Dictionary<Session,IEventsService> _eventsServicedDictionary = new();
    private ISessionService _sessionService;
    private readonly Dictionary<Session,ITasksService> _tasksServicedDictionary = new();

    public ArmoniKClient(Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ArmoniKClient>();
    }

    public ObjectPool<ChannelBase> ChannelPool
        => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
            _loggerFactory);

    public Task<IBlobService> GetBlobService(Session session)
    {
        if (_blobServiceDictionary.TryGetValue(session, out var blobService))
        {
            return Task.FromResult(blobService);
        }
        _blobServiceDictionary[session] = BlobServiceFactory.CreateBlobService(ChannelPool, session, _loggerFactory);
        return Task.FromResult(_blobServiceDictionary[session]);
    }

    public Task<ISessionService> GetSessionService()
    {
        if (_sessionService is not null)
        {
            return Task.FromResult(_sessionService);
        }
        _sessionService = SessionServiceFactory.CreateSessionService(ChannelPool, _properties, _loggerFactory);
        return Task.FromResult(_sessionService);
    }

    public async Task<ITasksService> GetTasksService(Session session)
    {
        if (_tasksServicedDictionary.TryGetValue(session, out var taskService))
        {
            return taskService;
        }
        _tasksServicedDictionary[session] = TasksServiceFactory.CreateTaskService(ChannelPool, await GetBlobService(session), session, _loggerFactory);
        return _tasksServicedDictionary[session];
    }

    public Task<IEventsService> GetEventsService(Session session)
    {
        if (_eventsServicedDictionary.TryGetValue(session, out var eventsService))
        {
            return Task.FromResult(eventsService);
        }
        _eventsServicedDictionary[session] = EventsServiceFactory.CreateEventsService(ChannelPool, session, _loggerFactory);
        return Task.FromResult(_eventsServicedDictionary[session]);
    }
}