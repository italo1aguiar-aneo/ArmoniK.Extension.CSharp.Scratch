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
    public class SessionServiceFactory
    {
        public static ISessionService CreateSessionService(ChannelBase channel, Properties properties, ILoggerFactory loggerFactory = null)
            => new SessionService(channel, properties, loggerFactory);
    }
}