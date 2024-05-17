using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArmoniK.Extension.CSharp.Client.Factory;

public class TasksServiceFactory
{
    public static ITasksService CreateTaskService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory = null)
    {
        return new TasksService(channel, loggerFactory);
    }
}