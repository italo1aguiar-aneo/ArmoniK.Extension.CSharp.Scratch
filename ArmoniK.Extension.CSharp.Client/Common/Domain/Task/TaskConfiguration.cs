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

using ArmoniK.Api.gRPC.V1;

using Google.Protobuf.WellKnownTypes;

using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskConfiguration
{
  public TaskConfiguration(int                        maxRetries,
                           int                        priority,
                           string                     partitionId,
                           TimeSpan                   maxDuration,
                           Dictionary<string, string> options                  = null,
                           ApplicationConfiguration   applicationConfiguration = null)
  {
    MaxRetries               = maxRetries;
    Priority                 = priority;
    PartitionId              = partitionId;
    Options                  = options;
    MaxDuration              = maxDuration;
    ApplicationConfiguration = applicationConfiguration;
  }

  public int    MaxRetries  { get; set; }
  public int    Priority    { get; set; }
  public string PartitionId { get; set; }

  [CanBeNull]
  public Dictionary<string, string> Options { get; set; }

  public TimeSpan MaxDuration { get; set; }

  [CanBeNull]
  public ApplicationConfiguration ApplicationConfiguration { get; set; }

  public TaskOptions ToTaskOptions()
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

    if (ApplicationConfiguration != null)
    {
      taskOptions.ApplicationName      = ApplicationConfiguration?.ApplicationName;
      taskOptions.ApplicationService   = ApplicationConfiguration?.ApplicationService;
      taskOptions.ApplicationNamespace = ApplicationConfiguration?.ApplicationNamespace;
      taskOptions.ApplicationVersion   = ApplicationConfiguration?.ApplicationVersion;
    }

    return taskOptions;
  }
}
