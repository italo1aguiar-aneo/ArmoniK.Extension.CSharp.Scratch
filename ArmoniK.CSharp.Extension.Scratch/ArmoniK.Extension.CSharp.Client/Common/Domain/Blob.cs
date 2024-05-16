using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class Blob : IBlobInfo
{
    public Blob(string blobName)
    {
        Name = blobName;
    }

    public Blob(string blobName, string blobId)
    {
        Name = blobName;
        BlobId = blobId;
    }

    public string Name { get; }

    public string BlobId { get; private set; }

    public ReadOnlyMemory<byte> Content { get; private set; }

    public void AddContent(ReadOnlyMemory<byte> content)
    {
        //add validations
        Content = content;
    }

    public void SetBlobId(string blobId)
    {
        if (BlobId == null)
            BlobId = blobId;
        else
            throw new InvalidOperationException("BlobId is already set and cannot be changed.");
    }
}