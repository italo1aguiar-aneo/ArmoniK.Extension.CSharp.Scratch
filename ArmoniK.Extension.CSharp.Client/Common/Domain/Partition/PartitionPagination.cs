using ArmoniK.Api.gRPC.V1.Partitions;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;

/// <summary>
///     Provides pagination capabilities for partition listings, including sorting and filtering functionalities.
/// </summary>
public class PartitionPagination
{
    /// <summary>
    ///     Current page number in the pagination query.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    ///     Number of items per page in the pagination query.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Direction in which the data should be sorted.
    /// </summary>
    public SortDirection SortDirection { get; set; }

    /// <summary>
    ///    Filters to be applied to the partition listing.
    /// </summary>
    public Filters Filter { get; set; }
}