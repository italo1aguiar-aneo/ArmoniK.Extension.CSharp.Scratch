using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class BlobInfo
{
    public BlobInfo(string name, string id, Session session)
    {
        Name = name;
        Id = id;
        Session = session;
    }

    public Session Session { get; }
    public string Name { get; }
    public string Id { get; }
}