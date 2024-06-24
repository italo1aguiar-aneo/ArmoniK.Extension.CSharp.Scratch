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

using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Factory;

/// <summary>
///   Provides a static factory method for creating instances of <see cref="IPartitionsService" />.
///   This service is responsible for managing data partitioning across different nodes or services.
/// </summary>
public static class PartitionsServiceFactory
{
  /// <summary>
  ///   Creates an instance of <see cref="IPartitionsService" /> using the specified GRPC channel and an optional logger
  ///   factory.
  /// </summary>
  /// <param name="channel">
  ///   An object pool that manages GRPC channels, providing efficient handling and reuse of channel
  ///   resources.
  /// </param>
  /// <param name="loggerFactory">
  ///   An optional factory for creating loggers, which can be used to enable logging within the
  ///   partitions service. If null, logging will be disabled.
  /// </param>
  /// <returns>An instance of <see cref="IPartitionsService" /> configured with the provided parameters.</returns>
  public static IPartitionsService CreatePartitionsService(ObjectPool<ChannelBase> channel,
                                                           ILoggerFactory          loggerFactory = null)
    => new PartitionsService(channel,
                             loggerFactory);
}
