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

    //should blobInfo have a session ? -> problem: BlobService will have a session already
    //, and it might be ambiguous on functions where we receive the session as parameter
    public Session Session { get; }
    public string Name { get; }
    public string Id { get; }
}