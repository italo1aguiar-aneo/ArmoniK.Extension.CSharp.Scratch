using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

/// <summary>
///     Factory for creating a VersionService
/// </summary>
public static class VersionsServiceFactory
{
    /// <summary>
    ///     Method for creating a new VersionService
    /// </summary>
    /// <param name="channel">Grpc Channel</param>
    /// <param name="loggerFactory">Optional LoggerFactory</param>
    /// <returns></returns>
    public static IVersionsService CreateVersionsService(ObjectPool<ChannelBase> channel,
        ILoggerFactory loggerFactory = null)
    {
        return new VersionsService(channel, loggerFactory);
    }
}