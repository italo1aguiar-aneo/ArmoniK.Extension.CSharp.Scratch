using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Partitions;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

/// <summary>
///     Defines a service for managing partitions, including retrieval and listing of partitions.
/// </summary>
public interface IPartitionsService
{
    /// <summary>
    ///     Asynchronously retrieves a partition by its identifier.
    /// </summary>
    /// <param name="partitionId">The identifier of the partition to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the partition information.</returns>
    Task<Partition> GetPartitionAsync(string partitionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Asynchronously lists partitions based on pagination options.
    /// </summary>
    /// <param name="partitionPagination">The options for pagination, including page number, page size, and sorting.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of tuples containing the total count and partition information.</returns>
    IAsyncEnumerable<(int, Partition)> ListPartitionsAsync(PartitionPagination partitionPagination,
        CancellationToken cancellationToken);
}

/// <summary>
///     Provides extension methods for the <see cref="IPartitionsService" /> interface.
/// </summary>
public static class PartitionsServiceExt
{
    /// <summary>
    ///     Asynchronously lists all partitions with support for pagination.
    /// </summary>
    /// <param name="partitionService">The partition service instance.</param>
    /// <param name="pageSize">The number of partitions to retrieve per page.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of partitions.</returns>
    public static async IAsyncEnumerable<Partition> ListAllPartitionsAsync(this IPartitionsService partitionService,
        int pageSize = 50, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var partitionPagination = new PartitionPagination
        {
            Filter = new Filters(),
            Page = 0,
            PageSize = pageSize,
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