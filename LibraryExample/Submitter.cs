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
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.DllHelper;
using ArmoniK.Extension.CSharp.DllCommon;
using ArmoniK.Extension.CSharp.Worker;

using Microsoft.Extensions.Logging;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace LibraryExample;

public class Submitter : IDllWorker
{
  public async Task<Output> Execute(ITaskHandler      taskHandler,
                                    ILogger           logger,
                                    CancellationToken cancellationToken)
  {
    var resultIds = await SubmitWorkers(taskHandler,
                                        logger,
                                        cancellationToken);
    await SubmitJoiner(taskHandler,
                       logger,
                       resultIds,
                       cancellationToken);
    return new Output
           {
             Ok = new Empty(),
           };
  }

  private async Task<List<string>> SubmitWorkers(ITaskHandler      taskHandler,
                                                 ILogger           logger,
                                                 CancellationToken cancellationToken)
  {
    try
    {
      logger.LogInformation("Submitting Workers");

      var input = Encoding.ASCII.GetString(taskHandler.Payload);

      var taskConfig = taskHandler.TaskOptions.GetTaskLibraryDefinition(taskHandler.TaskOptions.GetServiceLibrary());

      var taskLibDef = new TaskLibraryDefinition(taskConfig,
                                                 taskConfig.Namespace,
                                                 "Worker");

      var subTaskResults = taskHandler.CreateMetadataBlobs(Enumerable.Range(1,
                                                                            5)
                                                                     .Select(i => Guid.NewGuid() + "_" + i)
                                                                     .ToList(),
                                                           cancellationToken);
      var subTasksResultIds = new List<string>();

      await foreach (var subTask in subTaskResults)
      {
        subTasksResultIds.Add(subTask.BlobId);
      }

      var payload = taskHandler.CreateBlobs(new List<KeyValuePair<string, ReadOnlyMemory<byte>>>
                                            {
                                              new("Payload",
                                                  Encoding.ASCII.GetBytes($"{input}_FatherId_{taskHandler.TaskId}")),
                                            },
                                            cancellationToken);

      var payloadId = payload.ToEnumerable()
                             .Single()
                             .BlobId;

      logger.LogInformation("Submitting Workers tasks");

      await taskHandler.SubmitTasks(subTasksResultIds.Select(x => new TaskNodeExt
                                                                  {
                                                                    Payload = payload.ToEnumerable()
                                                                                     .First(),
                                                                    ExpectedOutputs = new List<BlobInfo>
                                                                                      {
                                                                                        new()
                                                                                        {
                                                                                          BlobId    = x,
                                                                                          BlobName  = " ",
                                                                                          SessionId = taskHandler.SessionId,
                                                                                        },
                                                                                      },
                                                                    DynamicLibrary = taskLibDef,
                                                                  }),
                                    cancellationToken);

      return subTasksResultIds;
    }
    catch (Exception e)
    {
      logger.LogError(e.Message);
      throw;
    }
  }

  private async Task SubmitJoiner(ITaskHandler        taskHandler,
                                  ILogger             logger,
                                  IEnumerable<string> expectedOutputIds,
                                  CancellationToken   cancellationToken)

  {
    logger.LogInformation("Submitting Joiner");

    var taskConfig = taskHandler.TaskOptions.GetTaskLibraryDefinition(taskHandler.TaskOptions.GetServiceLibrary());

    var taskLibDef = new TaskLibraryDefinition(taskConfig,
                                               taskConfig.Namespace,
                                               "Joiner");

    var subTaskResultId = taskHandler.ExpectedResults.Single();

    var payload = taskHandler.CreateBlobs(new List<KeyValuePair<string, ReadOnlyMemory<byte>>>
                                          {
                                            new("Payload",
                                                "Submiting Joiner"u8.ToArray()),
                                          });

    var payloadId = payload.ToEnumerable()
                           .Single()
                           .BlobId;

    logger.LogInformation("Submitting Joiner tasks");

    var dataDependencies = expectedOutputIds.Select(x => new BlobInfo
                                                         {
                                                           BlobId    = x,
                                                           BlobName  = " ",
                                                           SessionId = taskHandler.SessionId,
                                                         })
                                            .ToList();

    var expectedOutputs = new List<BlobInfo>
                          {
                            new()
                            {
                              BlobId    = subTaskResultId,
                              BlobName  = " ",
                              SessionId = taskHandler.SessionId,
                            },
                          };

    await taskHandler.SubmitTasks(new List<TaskNodeExt>
                                  {
                                    new()
                                    {
                                      Payload = payload.ToEnumerable()
                                                       .First(),
                                      ExpectedOutputs  = expectedOutputs,
                                      DataDependencies = dataDependencies,
                                      DynamicLibrary   = taskLibDef,
                                    },
                                  },
                                  cancellationToken);
  }
}
