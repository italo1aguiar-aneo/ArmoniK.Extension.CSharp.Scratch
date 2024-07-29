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

public class Joiner : IDllWorker
{
  public async Task<Output> Execute(ITaskHandler      taskHandler,
                                    ILogger           logger,
                                    CancellationToken cancellationToken)
  {
    logger.LogDebug("Starting Joiner useCase");
    var resultId = taskHandler.ExpectedResults.First();

    logger.LogDebug($"number of expectedOutputs:{taskHandler.ExpectedResults}");

    var resultsArray = taskHandler.DataDependencies.Values.Select(dependency => Encoding.ASCII.GetString(dependency))
                                  .Select(result => $"{result}_Joined")
                                  .ToList();

    await taskHandler.SendBlob(resultId,
                               resultsArray.SelectMany(s => Encoding.ASCII.GetBytes(s + "\n"))
                                           .ToArray(),
                               cancellationToken);
    return new Output
           {
             Ok = new Empty(),
           };
  }
}
