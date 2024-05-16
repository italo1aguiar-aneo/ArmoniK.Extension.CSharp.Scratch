using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory
{
    public class EventsServiceFactory
    {
        public static IEventsService CreateEventsService(ChannelBase channel, ILoggerFactory loggerFactory = null)
            => new EventsService(channel, loggerFactory);
    }
}
