using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ArmoniK.Api.gRPC.V1.HealthChecks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Health;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using HealthStatusEnum = ArmoniK.Extension.CSharp.Client.Common.Domain.Health.HealthStatusEnum;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class HealthCheckService : IHealthCheckService
{
    private readonly ObjectPool<ChannelBase> _channelPool;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<HealthCheckService>();
    }

    public async IAsyncEnumerable<Health> GetHealth([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var healthClient = new HealthChecksService.HealthChecksServiceClient(channel);

        var healthResponse = await healthClient.CheckHealthAsync(new CheckHealthRequest());

        foreach (var health in healthResponse.Services)
            yield return new Health
            {
                Name = health.Name,
                Message = health.Message,
                Status = (HealthStatusEnum)health.Healthy
            };
    }
}