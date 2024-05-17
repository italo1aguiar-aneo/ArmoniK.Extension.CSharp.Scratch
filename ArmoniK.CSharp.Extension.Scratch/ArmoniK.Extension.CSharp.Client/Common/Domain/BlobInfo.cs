using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class BlobInfo
{
    public BlobInfo(string name, string blobId)
    {
        Name = name;
        BlobId = blobId;
    }

    public string Name { get; }
    public string BlobId { get; }
}