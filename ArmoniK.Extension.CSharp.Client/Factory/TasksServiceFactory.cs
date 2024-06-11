using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

public static class TasksServiceFactory
{
    public static ITasksService CreateTaskService(ObjectPool<ChannelBase> channel, IBlobService blobService,
        ILoggerFactory loggerFactory = null)
    {
        return new TasksService(channel, blobService, loggerFactory);
    }
}