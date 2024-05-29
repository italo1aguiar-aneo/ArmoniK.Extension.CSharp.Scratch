namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public class BlobInfo
{
    public BlobInfo(string name, string id, string sessionId)
    {
        Name = name;
        Id = id;
        SessionId = sessionId;
    }

    protected BlobInfo()
    {
    }

    public string SessionId { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
}