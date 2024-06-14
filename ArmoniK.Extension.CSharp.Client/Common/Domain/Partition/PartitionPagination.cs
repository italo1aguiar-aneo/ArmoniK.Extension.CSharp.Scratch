using ArmoniK.Api.gRPC.V1.Partitions;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;

public class PartitionPagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public SortDirection SortDirection { get; set; }
    public Filters Filter { get; set; }
}