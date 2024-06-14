using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

/// <summary>
///     Provides a factory method to create instances of the task service.
/// </summary>
public static class TasksServiceFactory
{
    /// <summary>
    ///     Creates an instance of <see cref="ITasksService" /> using the specified GRPC channel, blob service, and an optional
    ///     logger factory.
    /// </summary>
    /// <param name="channel">An object pool that manages GRPC channels. This provides efficient handling of channel resources.</param>
    /// <param name="blobService">The blob service to be used for blob manipulation operations within the task service.</param>
    /// <param name="loggerFactory">
    ///     An optional logger factory to enable logging within the task service. If null, logging will
    ///     be disabled.
    /// </param>
    /// <returns>An instance of <see cref="ITasksService" /> that can be used to perform task operations.</returns>
    public static ITasksService CreateTaskService(ObjectPool<ChannelBase> channel, IBlobService blobService,
        ILoggerFactory loggerFactory = null)
    {
        return new TasksService(channel, blobService, loggerFactory);
    }
}