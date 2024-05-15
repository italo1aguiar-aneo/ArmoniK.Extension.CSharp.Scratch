using ArmoniK.Api.gRPC.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Utils;
using Grpc.Core;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;

namespace ArmoniK.Extension.CSharp.Client
{
    public class ArmoniKClient
    {
        public readonly IBlobService BlobService;
        public readonly ITaskService TasksService;
        public readonly ISessionService SessionService;
        public readonly IEventService EventsService;

        public ArmoniKClient(Properties properties, ILoggerFactory logger)
        {
            BlobService = BlobServiceFactory.CreateBlobService(properties, logger);
            TasksService = TaskServiceFactory.CreateTaskService(properties, logger);
            SessionService = SessionServiceFactory.CreateSessionService(properties, logger);
            EventsService = EventsServiceFactory.CreateSessionService(properties, logger);
        }
    }
}