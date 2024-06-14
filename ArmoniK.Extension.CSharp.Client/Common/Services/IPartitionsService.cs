using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Partitions;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IPartitionsService
{
    Task<Partition> GetPartitionAsync(string partitionId, CancellationToken cancellationToken);

    IAsyncEnumerable<(int, Partition)> ListPartitionsAsync(PartitionPagination partitionPagination,
        CancellationToken cancellationToken);
}

public static class IPartitionsServiceExt
{
    public static async IAsyncEnumerable<Partition> ListAllPartitionsAsync(this IPartitionsService partitionService,
        int pageSize = 50, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var partitionPagination = new PartitionPagination
        {
            Filter = new Filters(),
            Page = 0,
            PageSize = 50,
            SortDirection = SortDirection.Asc
        };

        var total = 0;
        var firstPage = true;

        while (true)
        {
            await foreach (var (count, partition) in partitionService.ListPartitionsAsync(partitionPagination,
                               cancellationToken))
            {
                if (firstPage)
                {
                    total = count;
                    firstPage = false;
                }

                yield return partition;
            }

            partitionPagination.Page++;
            if (partitionPagination.Page * pageSize >= total) break;
        }
    }
}