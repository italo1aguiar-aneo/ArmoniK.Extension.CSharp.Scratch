using System;
using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class Blob : BlobInfo
{
    public Blob(string blobName, string id, Session session) : base(blobName, id, session)
    {
    }

    public ReadOnlyMemory<byte> Content { get; private set; }

    public void AddContent(ReadOnlyMemory<byte> content)
    {
        //add validations
        Content = content;
    }
}