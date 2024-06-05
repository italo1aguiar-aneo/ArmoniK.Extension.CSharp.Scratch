namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public class BlobInfo
{
    public BlobInfo(string blobName, string blobId, string sessionId)
    {
        BlobName = blobName;
        BlobId = blobId;
        SessionId = sessionId;
    }

    protected BlobInfo()
    {
    }

    public string SessionId { get; set; }
    public string BlobName { get; set; }
    public string BlobId { get; set; }
}