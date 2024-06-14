namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

/// <summary>
///     Represents information that minimally defines a blob.
/// </summary>
public record BlobInfo
{
    /// <summary>
    ///     Session ID associated with the blob.
    /// </summary>
    public string SessionId { get; init; }

    /// <summary>
    ///     Name of the blob.
    /// </summary>
    public string BlobName { get; init; }

    /// <summary>
    ///     Blob unique identifier.
    /// </summary>
    public string BlobId { get; init; }
}