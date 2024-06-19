using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskNode
{
  public IEnumerable<BlobInfo> ExpectedOutputs { get; set; }

  public ICollection<BlobInfo> DataDependencies { get; set; } = new List<BlobInfo>();

  public IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> DataDependenciesContent { get; set; } = ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

  public BlobInfo Payload { get; set; } = null;

  public KeyValuePair<string, ReadOnlyMemory<byte>> PayloadContent { get; set; } = new(string.Empty,
                                                                                       ReadOnlyMemory<byte>.Empty);

  [CanBeNull]
  public TaskConfiguration TaskOptions { get; set; }

  public SessionInfo Session { get; set; }
}
