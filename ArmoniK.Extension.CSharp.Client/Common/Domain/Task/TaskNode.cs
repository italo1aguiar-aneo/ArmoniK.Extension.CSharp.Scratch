using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public record TaskNode
{
    public IEnumerable<BlobInfo> ExpectedOutputs { get; init; }

    public ICollection<BlobInfo> DataDependencies { get; init; } = new List<BlobInfo>();

    public IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> DataDependenciesContent { get; init; } =
        ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

    public BlobInfo Payload { get; set; } = null;

    public KeyValuePair<string, ReadOnlyMemory<byte>> PayloadContent { get; init; } =
        new(string.Empty, ReadOnlyMemory<byte>.Empty);

    [CanBeNull] public TaskConfiguration TaskOptions { get; init; }
    public SessionInfo Session { get; init; }
}