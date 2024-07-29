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

using System.Text;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.DllCommon;
using ArmoniK.Extension.CSharp.Worker;

using Microsoft.Extensions.Logging;

namespace LibraryExample;

public class Worker : IDllWorker
{
  public async Task<Output> Execute(ITaskHandler      taskHandler,
                                    ILogger           logger,
                                    CancellationToken cancellationToken)
  {
    logger.LogWarning("Starting HelloWorker");
    var input = Encoding.ASCII.GetString(taskHandler.Payload);

    var resultId = taskHandler.ExpectedResults.First();

    logger.LogDebug($"number of expectedOutputs:{taskHandler.ExpectedResults}");
    // We add the SubTaskId to the result 
    await taskHandler.SendBlob(resultId,
                               Encoding.ASCII.GetBytes($"{input}_SonId_{taskHandler.TaskId}"),
                               cancellationToken)
                     .ConfigureAwait(false);
    return new Output
           {
             Ok = new Empty(),
           };
  }
}
