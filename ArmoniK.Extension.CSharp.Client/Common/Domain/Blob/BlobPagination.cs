using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.SortDirection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

/// <summary>
///     Provides pagination capabilities for listing blobs, including sorting and filtering functionalities.
/// </summary>
public class BlobPagination
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
    ///     filters to be applied to the blob listing.
    /// </summary>
    public Filters Filter { get; set; }
}

/// <summary>
///     Represents a page of blob information in a paginated list.
/// </summary>
public record BlobPage
{
    /// <summary>
    ///     Total number of pages available.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    ///     Details of the blob on this page.
    /// </summary>
    public BlobState BlobDetails { get; init; }
}