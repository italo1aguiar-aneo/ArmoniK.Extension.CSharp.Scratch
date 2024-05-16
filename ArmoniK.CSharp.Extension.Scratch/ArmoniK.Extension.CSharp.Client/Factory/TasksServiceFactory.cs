using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArmoniK.Extension.CSharp.Client.Factory
{
    public class TasksServiceFactory
    {
        public static ITasksService CreateTaskService(ChannelBase channel,ILoggerFactory loggerFactory = null)
            => new TasksService(channel, loggerFactory);
    }
}