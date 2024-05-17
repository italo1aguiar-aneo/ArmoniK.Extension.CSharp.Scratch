using System.Threading.Tasks;
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
    private IBlobService _blobService;
    private ObjectPool<ChannelBase> _channelPool;
    private IEventsService _eventsService;
    private ISessionService _sessionService;
    private ITasksService _tasksService;

    public ArmoniKClient(Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ArmoniKClient>();
    }

    public ObjectPool<ChannelBase> ChannelPool
        => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
            _loggerFactory);

    public async Task<IBlobService> GetBlobService()
    {
        if (_blobService is not null) return _blobService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _blobService = BlobServiceFactory.CreateBlobService(ChannelPool, _loggerFactory);
        return _blobService;
    }

    public async Task<ISessionService> GetSessionService()
    {
        if (_sessionService is not null) return _sessionService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _sessionService = SessionServiceFactory.CreateSessionService(ChannelPool, _properties, _loggerFactory);
        return _sessionService;
    }

    public async Task<ITasksService> GetTasksService()
    {
        if (_tasksService is not null) return _tasksService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _tasksService = TasksServiceFactory.CreateTaskService(ChannelPool, await GetBlobService(), _loggerFactory);
        return _tasksService;
    }

    public async Task<IEventsService> GetEventsService()
    {
        if (_eventsService is not null) return _eventsService;
        ChannelBase channel = await ChannelPool.GetAsync();
        _eventsService = EventsServiceFactory.CreateEventsService(ChannelPool, _loggerFactory);
        return _eventsService;
    }
}