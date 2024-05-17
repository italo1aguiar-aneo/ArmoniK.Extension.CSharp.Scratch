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

    // Choix de desgin à faire
    public IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> DataDependenciesContent { get; set; } =
        ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

    public BlobInfo Payload { get; set; } = null;

    // Choix de desgin à faire
    public KeyValuePair<string, ReadOnlyMemory<byte>> PayloadContent { get; set; } =
        new(string.Empty, ReadOnlyMemory<byte>.Empty);

    [CanBeNull] public TaskOptions TaskOptions { get; set; }
}