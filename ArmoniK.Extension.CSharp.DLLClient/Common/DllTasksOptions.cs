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
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Utils;

using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace ArmoniK.Extension.CSharp.DLLClient.Common;

public record DllTasksOptions : TaskConfiguration
{
  public DllTasksOptions(IEnumerable<DynamicLibrary> dynamicLibraries,
                         int                         maxRetries,
                         int                         priority,
                         string                      partitionId,
                         TimeSpan                    maxDuration,
                         Dictionary<string, string>  options = null)
    : base(maxRetries,
           priority,
           partitionId,
           maxDuration,
           options)
    => DynamicLibraries = dynamicLibraries;

  public DllTasksOptions(IEnumerable<DynamicLibrary> dynamicLibraries,
                         TaskConfiguration           taskConfiguration)
    : base(taskConfiguration.MaxRetries,
           taskConfiguration.Priority,
           taskConfiguration.PartitionId,
           taskConfiguration.MaxDuration,
           taskConfiguration.Options)
    => DynamicLibraries = dynamicLibraries;

  public IEnumerable<DynamicLibrary> DynamicLibraries { get; init; }

  /// <summary>
  ///   Converts this <see cref="TaskConfiguration" /> instance to a <see cref="TaskOptions" /> suitable for use with task
  ///   submission.
  /// </summary>
  /// <returns>A new <see cref="TaskOptions" /> instance populated with the settings from this configuration.</returns>
  public new TaskOptions ToTaskOptions()
  {
    var taskOptions = new TaskOptions
                      {
                        MaxRetries  = MaxRetries,
                        Priority    = Priority,
                        PartitionId = PartitionId,
                        MaxDuration = Duration.FromTimeSpan(MaxDuration),
                      };

    if (Options != null)
    {
      taskOptions.Options.Add(Options);
    }

    if (DynamicLibraries.Any())
    {
      foreach(var lib in DynamicLibraries)
      {
        taskOptions.Options.Add(new MapField<string, string>
                             {
                               Keys =
                               {
                                 lib.ToString(),
                               },
                               Values =
                               {
                                 JsonSerializer.Serialize(lib),
                               },
                             });
      }
    }

    return taskOptions;
  }
}
