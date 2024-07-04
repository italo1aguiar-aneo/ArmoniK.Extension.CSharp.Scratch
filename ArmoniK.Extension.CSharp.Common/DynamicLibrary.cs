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

namespace ArmoniK.Extension.CSharp.Common;

/// <summary>
///   Represents the configuration of an application, including its name, version, namespace, service, and engine type.
/// </summary>
public record DynamicLibrary
{
  /// <summary>
  ///   Name of the application.
  /// </summary>
  public string Name { get; init; }

  /// <summary>
  ///   FileName of the Dll.
  /// </summary>
  public string DllFileName { get; init; }

  /// <summary>
  ///   Version of the application.
  /// </summary>
  public string PathToFile { get; init; }

  /// <summary>
  ///   Version of the application.
  /// </summary>
  public string Version { get; init; }

  public override string ToString()
    => $"{Name}:{Version}.dll";
}
