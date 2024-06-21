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

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Options;
using ArmoniK.Api.Worker.Worker;

namespace ArmoniK.Extension.CSharp.DllWorker.Services;

public class ComputerService : WorkerStreamWrapper
{
  public ComputerService(IConfiguration      configuration,
                         ILoggerFactory      loggerFactory,
                         ComputePlane        computePlaneOptions,
                         GrpcChannelProvider provider)
    : base(loggerFactory,
           computePlaneOptions,
           provider)
  {
    Logger        = loggerFactory.CreateLogger<ComputerService>();
    Configuration = configuration;
  }

  private ILogger<ComputerService> Logger { get; }

  public IConfiguration Configuration { get; }
}
