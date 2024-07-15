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
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.DllCommon;
using ArmoniK.Extension.CSharp.Worker;

namespace ArmoniK.Extension.CSharp.DllWorker;

public class ServiceRequestContext
{
  private readonly ILibraryLoader                 libraryLoader_;
  private readonly ILibraryWorker                 libraryWorker_;
  private readonly ILogger<ServiceRequestContext> logger_;

  private string currentSession_;

  public ServiceRequestContext(IConfiguration configuration,
                               ILoggerFactory loggerFactory)
  {
    LoggerFactory = loggerFactory;

    logger_ = loggerFactory.CreateLogger<ServiceRequestContext>();

    libraryLoader_ = new LibraryLoader(loggerFactory);
    libraryWorker_ = new LibraryWorker(configuration,
                                       loggerFactory);
  }

  public ILoggerFactory LoggerFactory { get; set; }

  public async Task<Output> ExecuteTask(ITaskHandler      taskHandler,
                                        CancellationToken cancellationToken)
  {
    if (currentSession_ == default || taskHandler.SessionId != currentSession_)
    {
      currentSession_ = taskHandler.SessionId;
      libraryLoader_.ResetService();
    }

    var contextName = await libraryLoader_.LoadLibrary(taskHandler,
                                                       cancellationToken);

    var result = await libraryWorker_.Execute(taskHandler,
                                              libraryLoader_,
                                              contextName,
                                              cancellationToken);
    return result;
  }
}
