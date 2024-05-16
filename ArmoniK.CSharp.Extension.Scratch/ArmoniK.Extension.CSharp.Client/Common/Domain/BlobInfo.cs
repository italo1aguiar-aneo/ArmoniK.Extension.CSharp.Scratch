using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class BlobInfo : IBlobInfo
{
    public BlobInfo(string name)
    {
        Name = name;
    }

    public BlobInfo(string name, string blobId)
    {
        Name = name;
        BlobId = blobId;
    }

    public string Name { get; }
    public string BlobId { get; }
}