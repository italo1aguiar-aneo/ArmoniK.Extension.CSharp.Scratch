using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ArmoniK.Api.gRPC.V1;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class TaskNode
{
    public IEnumerable<BlobInfo> ExpectedOutputs { get; set; }
    public ICollection<BlobInfo> DataDependencies { get; set; } = new List<BlobInfo>();
    // echange avec jerome et flo pour voir si on garde ça ou pas -- j'aime pas cette façon de faire...
    public IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> DataDependenciesContent { get; set; } =
        ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

    public BlobInfo Payload { get; set; }
    [CanBeNull] public TaskOptions TaskOptions { get; set; }
}