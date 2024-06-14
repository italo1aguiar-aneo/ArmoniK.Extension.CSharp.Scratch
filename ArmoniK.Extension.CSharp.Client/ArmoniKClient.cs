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

/// <summary>
///     Provides a client for interacting with the ArmoniK services, including blob, session, task, event, health check,
///     partition, and version services.
/// </summary>
public class ArmoniKClient
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Properties _properties;
    private IBlobService _blobService;
    private ObjectPool<ChannelBase> _channelPool;
    private IEventsService _eventsService;
    private IHealthCheckService _healthCheckService;
    private IPartitionsService _partitionsService;
    private ISessionService _sessionService;
    private ITasksService _tasksService;
    private IVersionsService _versionsService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArmoniKClient" /> class with the specified properties and logger
    ///     factory.
    /// </summary>
    /// <param name="properties">The properties for configuring the client.</param>
    /// <param name="loggerFactory">The factory for creating loggers.</param>
    /// <exception cref="ArgumentNullException">Thrown when properties or loggerFactory is null.</exception>
    public ArmoniKClient(Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.CreateLogger<ArmoniKClient>();
    }

    /// <summary>
    ///     Gets the channel pool used for managing GRPC channels.
    /// </summary>
    public ObjectPool<ChannelBase> ChannelPool
        => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
            _loggerFactory);

    /// <summary>
    ///     Gets the blob service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the blob service instance.</returns>
    public Task<IBlobService> GetBlobService()
    {
        if (_blobService is not null) return Task.FromResult(_blobService);
        _blobService = BlobServiceFactory.CreateBlobService(ChannelPool, _loggerFactory);
        return Task.FromResult(_blobService);
    }

    /// <summary>
    ///     Gets the session service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the session service instance.</returns>
    public Task<ISessionService> GetSessionService()
    {
        if (_sessionService is not null) return Task.FromResult(_sessionService);
        _sessionService = SessionServiceFactory.CreateSessionService(ChannelPool, _properties, _loggerFactory);
        return Task.FromResult(_sessionService);
    }

    /// <summary>
    ///     Gets the tasks service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the tasks service instance.</returns>
    public async Task<ITasksService> GetTasksService()
    {
        if (_tasksService is not null) return _tasksService;
        _tasksService =
            TasksServiceFactory.CreateTaskService(ChannelPool, await GetBlobService(), _loggerFactory);
        return _tasksService;
    }

    /// <summary>
    ///     Gets the events service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the events service instance.</returns>
    public Task<IEventsService> GetEventsService()
    {
        if (_eventsService is not null)
            return Task.FromResult(_eventsService);
        _eventsService =
            EventsServiceFactory.CreateEventsService(ChannelPool, _loggerFactory);
        return Task.FromResult(_eventsService);
    }

    /// <summary>
    ///     Gets the version service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the version service instance.</returns>
    public Task<IVersionsService> GetVersionService()
    {
        if (_versionsService is not null)
            return Task.FromResult(_versionsService);
        _versionsService =
            VersionsServiceFactory.CreateVersionsService(ChannelPool, _loggerFactory);
        return Task.FromResult(_versionsService);
    }

    /// <summary>
    ///     Gets the partitions service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the partitions service instance.</returns>
    public Task<IPartitionsService> GetPartitionsService()
    {
        if (_partitionsService is not null)
            return Task.FromResult(_partitionsService);
        _partitionsService =
            PartitionsServiceFactory.CreatePartitionsService(ChannelPool, _loggerFactory);
        return Task.FromResult(_partitionsService);
    }

    /// <summary>
    ///     Gets the health check service.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the health check service instance.</returns>
    public Task<IHealthCheckService> GetHealthCheckService()
    {
        if (_healthCheckService is not null)
            return Task.FromResult(_healthCheckService);
        _healthCheckService =
            HealthCheckServiceFactory.CreateHealthCheckService(ChannelPool, _loggerFactory);
        return Task.FromResult(_healthCheckService);
    }

    /// <summary>
    ///     Gets a blob handler for the specified blob information.
    /// </summary>
    /// <param name="blobInfo">The blob information.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the blob handler instance.</returns>
    public Task<BlobHandler> GetBlobHandler(BlobInfo blobInfo)
    {
        return Task.FromResult(new BlobHandler(blobInfo, this));
    }

    /// <summary>
    ///     Gets a task handler for the specified task information.
    /// </summary>
    /// <param name="taskInfos">The task information.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the task handler instance.</returns>
    public Task<TaskHandler> GetTaskHandler(TaskInfos taskInfos)
    {
        return Task.FromResult(new TaskHandler(this, taskInfos));
    }

    /// <summary>
    ///     Gets a session handler for the specified session information.
    /// </summary>
    /// <param name="session">The session information.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the session handler instance.</returns>
    public Task<SessionHandler> GetSessionHandler(SessionInfo session)
    {
        return Task.FromResult(new SessionHandler(session, this));
    }
}