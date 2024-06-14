using ArmoniK.Api.gRPC.V1.Results;
using SortDirection = ArmoniK.Api.gRPC.V1.SortDirection.SortDirection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public class BlobPagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public SortDirection SortDirection { get; set; }
    public Filters Filter { get; set; }
}

public record BlobPage
{
    public int TotalPages { get; init; }
    public BlobState BlobDetails { get; init; }
}