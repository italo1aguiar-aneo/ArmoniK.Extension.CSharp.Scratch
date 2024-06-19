// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2024. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
