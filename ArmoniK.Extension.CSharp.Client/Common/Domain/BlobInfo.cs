using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class BlobInfo
{
    public BlobInfo(string name, string id, string sessionId)
    {
        Name = name;
        Id = id;
        SessionId = sessionId;
    }
    public string SessionId { get; }
    public string Name { get; }
    public string Id { get; }
}