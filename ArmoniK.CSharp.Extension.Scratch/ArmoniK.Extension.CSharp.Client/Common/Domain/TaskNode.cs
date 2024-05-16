using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain
{
    public class TaskNode
    {
        public IEnumerable<BlobInfo> ExpectedOutputs { get; set; }
        public IEnumerable<BlobInfo> DataDependencies { get; set; }
        public BlobInfo Payload { get; set; }
        [CanBeNull] public TaskOptions TaskOptions { get; set; }
    }
}