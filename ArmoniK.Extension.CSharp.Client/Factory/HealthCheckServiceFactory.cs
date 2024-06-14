using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

public static class HealthCheckServiceFactory
{
    public static IHealthCheckService CreateHealthCheckService(ObjectPool<ChannelBase> channel,
        ILoggerFactory loggerFactory = null)
    {
        return new HealthCheckService(channel, loggerFactory);
    }
}