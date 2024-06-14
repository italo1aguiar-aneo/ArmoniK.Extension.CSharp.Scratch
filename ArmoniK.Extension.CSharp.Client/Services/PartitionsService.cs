using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Partitions;
using ArmoniK.Api.gRPC.V1.SortDirection;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class PartitionsService : IPartitionsService
{
    private readonly ObjectPool<ChannelBase> _channel;
    private readonly ILogger<PartitionsService> _logger;

    public PartitionsService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PartitionsService>();
        _channel = channel;
    }

    public async Task<Partition> GetPartitionAsync(string partitionId, CancellationToken cancellationToken)
    {
        await using var channel = await _channel.GetAsync(cancellationToken).ConfigureAwait(false);
        var partitionsClient = new Partitions.PartitionsClient(channel);
        var partition = await partitionsClient.GetPartitionAsync(new GetPartitionRequest
        {
            Id = partitionId
        }, cancellationToken: cancellationToken);
        return new Partition
        {
            Id = partition.Partition.Id,
            ParentPartitionIds = partition.Partition.ParentPartitionIds,
            PodConfiguration = partition.Partition.PodConfiguration,
            PodMax = partition.Partition.PodMax,
            PodReserved = partition.Partition.PodReserved,
            PreemptionPercentage = partition.Partition.PreemptionPercentage,
            Priority = partition.Partition.Priority
        };
    }

    public async IAsyncEnumerable<(int, Partition)> ListPartitionsAsync(PartitionPagination partitionPagination,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var channel = await _channel.GetAsync(cancellationToken).ConfigureAwait(false);
        var partitionsClient = new Partitions.PartitionsClient(channel);
        var partitions = await partitionsClient.ListPartitionsAsync(new ListPartitionsRequest
        {
            Filters = partitionPagination.Filter,
            Page = partitionPagination.Page,
            PageSize = partitionPagination.PageSize,
            Sort = new ListPartitionsRequest.Types.Sort { Direction = (SortDirection)partitionPagination.SortDirection }
        });

        foreach (var partition in partitions.Partitions)
            yield return (partitions.Total, new Partition
            {
                Id = partition.Id,
                ParentPartitionIds = partition.ParentPartitionIds,
                PodConfiguration = partition.PodConfiguration,
                PodMax = partition.PodMax,
                PodReserved = partition.PodReserved,
                PreemptionPercentage = partition.PreemptionPercentage,
                Priority = partition.Priority
            });
        ;
    }
}