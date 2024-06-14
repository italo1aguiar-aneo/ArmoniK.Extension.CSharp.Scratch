using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

/// <summary>
///     Provides a factory method for creating instances of <see cref="IHealthCheckService" />.
/// </summary>
public static class HealthCheckServiceFactory
{
    /// <summary>
    ///     Creates an instance of <see cref="IHealthCheckService" /> using the specified GRPC channel and an optional logger
    ///     factory.
    /// </summary>
    /// <param name="channel">
    ///     An object pool that manages GRPC channels, providing efficient handling and reuse of channel
    ///     resources.
    /// </param>
    /// <param name="loggerFactory">
    ///     An optional factory for creating loggers, which can be used to enable logging within the
    ///     health check service. If null, logging will be disabled.
    /// </param>
    /// <returns>An instance of <see cref="IHealthCheckService" /> configured with the provided parameters.</returns>
    public static IHealthCheckService CreateHealthCheckService(ObjectPool<ChannelBase> channel,
        ILoggerFactory loggerFactory = null)
    {
        return new HealthCheckService(channel, loggerFactory);
    }
}