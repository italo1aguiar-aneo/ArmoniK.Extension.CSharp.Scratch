namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public record BlobInfo
{
    public string SessionId { get; init; }
    public string BlobName { get; init; }
    public string BlobId { get; init; }
}