using System;
using System.Collections.Generic;
using System.Text;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain
{
    public interface IBlobInfo
    {
        public string Name { get; }
        public string BlobId { get; }
    }
}