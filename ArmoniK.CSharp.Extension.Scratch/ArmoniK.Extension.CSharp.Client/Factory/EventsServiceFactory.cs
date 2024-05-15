using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory
{
    public class EventsServiceFactory
    {
        public static IEventService CreateSessionService(Properties props,
            ILoggerFactory loggerFactory = null)
            => new EventService(props,
                loggerFactory);
    }
}
