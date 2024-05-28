using System;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class Blob : BlobInfo
{
    public Blob(string blobName, string id, string sessionId) : base(blobName, id, sessionId)
    {
    }

    public ReadOnlyMemory<byte> Content { get; private set; }

    public void AddContent(ReadOnlyMemory<byte> content)
    {
        //add validations
        Content = content;
    }
}