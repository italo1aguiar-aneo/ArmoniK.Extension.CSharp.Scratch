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

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

namespace ArmoniK.Extension.CSharp.DllCommon;

public record TaskLibraryDefinition : DynamicLibrary
{
  public TaskLibraryDefinition()
  {
  }

  public TaskLibraryDefinition(DynamicLibrary dll,
                               string         space,
                               string         service)
    : base(dll)
  {
    Namespace = space;
    Service   = service;
  }

  /// <summary>
  ///   Namespace of the application.
  /// </summary>
  public string Namespace { get; init; }

  /// <summary>
  ///   Service name of the application.
  /// </summary>
  public string Service { get; init; }
}

public static class TaskLibraryDefinitionExt
{
  public static TaskConfiguration AddTaskLibraryDefinition(this TaskConfiguration taskConfiguration,
                                                           TaskLibraryDefinition  dynamicLibrary)
  {
    taskConfiguration.AddDynamicLibrary(dynamicLibrary);
    taskConfiguration.Options.Add($"{dynamicLibrary}.Namespace",
                                  dynamicLibrary.Namespace);
    taskConfiguration.Options.Add($"{dynamicLibrary}.Service",
                                  dynamicLibrary.Service);
    return taskConfiguration;
  }

  public static TaskLibraryDefinition GetTaskLibraryDefinition(this TaskOptions taskOptions,
                                                               string           libraryName)
  {
    var dll = taskOptions.GetDynamicLibrary(libraryName);

    taskOptions.Options.TryGetValue($"{libraryName}.Namespace",
                                    out var serviceNamespace);

    taskOptions.Options.TryGetValue($"{libraryName}.Service",
                                    out var service);
    return new TaskLibraryDefinition(dll,
                                     serviceNamespace,
                                     service);
  }
}
