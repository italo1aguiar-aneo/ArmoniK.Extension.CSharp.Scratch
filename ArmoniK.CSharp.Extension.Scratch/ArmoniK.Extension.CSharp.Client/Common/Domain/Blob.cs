using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class Blob : BlobInfo
{
    public Blob(string blobName, string blobId) : base(blobName, blobId)
    {
    }

    public ReadOnlyMemory<byte> Content { get; private set; }

    public void AddContent(ReadOnlyMemory<byte> content)
    {
        //add validations
        Content = content;
    }
}