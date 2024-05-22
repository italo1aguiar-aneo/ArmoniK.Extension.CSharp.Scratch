﻿using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

public class EventsServiceFactory
{
    public static IEventsService CreateEventsService(ObjectPool<ChannelBase> channel, Session session,
        ILoggerFactory loggerFactory = null)
    {
        return new EventsService(channel, session, loggerFactory);
    }
}