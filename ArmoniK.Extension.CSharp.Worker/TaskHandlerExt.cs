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

using System.Collections.Immutable;

using ArmoniK.Api.gRPC.V1.Agent;
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.DllHelper;
using ArmoniK.Extension.CSharp.Client.DllHelper.Common;
using ArmoniK.Extension.CSharp.DllCommon;

using Google.Protobuf;

namespace ArmoniK.Extension.CSharp.Worker;

public static class TaskHandlerExt
{
  public static async Task SubmitTasks(this ITaskHandler        taskHandler,
                                       IEnumerable<TaskNodeExt> taskNodes,
                                       DllBlob                  dllBlob, //must evaluate if we should tell this to him always
                                       CancellationToken        cancellationToken)
  {
    taskNodes = taskNodes.Select(x =>
                                 {
                                   x.DataDependencies.Add(dllBlob);

                                   //avoid injection of dlls which were already defined in the session taskOptions
                                   if (x.TaskOptions?.Options is not null && x.TaskOptions.Options.ContainsKey(dllBlob.BlobName))
                                   {
                                     x.TaskOptions.Options.Remove(dllBlob.BlobName);
                                   }

                                   x.TaskOptions.AddTaskLibraryDefinition(x.DynamicLibrary);
                                   x.TaskOptions.Options.Add("ServiceLibrary",
                                                             x.DynamicLibrary.ToString());
                                   return x;
                                 });

    await taskNodes.CreateNewBlobs(taskHandler,
                                   cancellationToken);

    var submitTasksRequest = taskNodes.Select(x => new SubmitTasksRequest.Types.TaskCreation
                                                   {
                                                     DataDependencies =
                                                     {
                                                       x.DataDependencies.Select(y => y.BlobId),
                                                     },
                                                     ExpectedOutputKeys =
                                                     {
                                                       x.ExpectedOutputs.Select(y => y.BlobId),
                                                     },
                                                     PayloadId   = x.Payload.BlobId,
                                                     TaskOptions = x.TaskOptions.ToTaskOptions(),
                                                   });
    await taskHandler.SubmitTasksAsync(submitTasksRequest,
                                       default,
                                       cancellationToken);
  }

  public static async Task SubmitTasks(this ITaskHandler        taskHandler,
                                       IEnumerable<TaskNodeExt> taskNodes,
                                       CancellationToken        cancellationToken)
  {
    taskNodes = taskNodes.Select(x =>
                                 {
                                   if (taskHandler.TaskOptions.Options.ContainsKey("ServiceLibrary"))
                                   {
                                     taskHandler.TaskOptions.Options.Remove("ServiceLibrary");
                                   }

                                   taskHandler.TaskOptions.Options.Add("ServiceLibrary",
                                                                       x.DynamicLibrary.ToString());
                                   if (taskHandler.TaskOptions.Options.ContainsKey($"{x.DynamicLibrary}.Namespace"))
                                   {
                                     taskHandler.TaskOptions.Options.Remove($"{x.DynamicLibrary}.Namespace");
                                   }

                                   taskHandler.TaskOptions.Options.Add($"{x.DynamicLibrary}.Namespace",
                                                                       x.DynamicLibrary.Namespace);
                                   if (taskHandler.TaskOptions.Options.ContainsKey($"{x.DynamicLibrary}.Service"))
                                   {
                                     taskHandler.TaskOptions.Options.Remove($"{x.DynamicLibrary}.Service");
                                   }

                                   taskHandler.TaskOptions.Options.Add($"{x.DynamicLibrary}.Service",
                                                                       x.DynamicLibrary.Service);
                                   return x;
                                 });

    await taskNodes.CreateNewBlobs(taskHandler,
                                   cancellationToken);

    var submitTasksRequest = taskNodes.Select(x => new SubmitTasksRequest.Types.TaskCreation
                                                   {
                                                     DataDependencies =
                                                     {
                                                       x.DataDependencies.Select(y => y.BlobId),
                                                     },
                                                     ExpectedOutputKeys =
                                                     {
                                                       x.ExpectedOutputs.Select(y => y.BlobId),
                                                     },
                                                     PayloadId   = x.Payload.BlobId,
                                                     TaskOptions = taskHandler.TaskOptions,
                                                   });
    await taskHandler.SubmitTasksAsync(submitTasksRequest,
                                       default,
                                       cancellationToken);
  }

  public static async IAsyncEnumerable<BlobInfo> CreateBlobs(this ITaskHandler                                       taskHandler,
                                                             IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
                                                             CancellationToken                                       cancellationToken = default)
  {
    var submitBlobsRequest = blobKeyValuePairs.Select(x => new CreateResultsRequest.Types.ResultCreate
                                                           {
                                                             Name = x.Key,
                                                             Data = UnsafeByteOperations.UnsafeWrap(x.Value),
                                                           });
    var createResponse = await taskHandler.CreateResultsAsync(submitBlobsRequest);

    foreach (var newBlob in createResponse.Results)
    {
      yield return new BlobInfo
                   {
                     BlobId    = newBlob.ResultId,
                     BlobName  = newBlob.Name,
                     SessionId = newBlob.SessionId,
                   };
    }
  }

  public static async IAsyncEnumerable<BlobInfo> CreateMetadataBlobs(this ITaskHandler   taskHandler,
                                                                     IEnumerable<string> names,
                                                                     CancellationToken   cancellationToken = default)
  {
    var createBlobsMetadataRequest = names.Select(x => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                       {
                                                         Name = x,
                                                       });
    var createMetadataResponse = await taskHandler.CreateResultsMetaDataAsync(createBlobsMetadataRequest,
                                                                              cancellationToken);

    foreach (var newBlobMetadata in createMetadataResponse.Results)
    {
      yield return new BlobInfo
                   {
                     BlobId    = newBlobMetadata.ResultId,
                     BlobName  = newBlobMetadata.Name,
                     SessionId = newBlobMetadata.SessionId,
                   };
    }
  }

  public static async Task SendBlob(this ITaskHandler    taskHandler,
                                    string               blobId,
                                    ReadOnlyMemory<byte> content,
                                    CancellationToken    cancellationToken = default)
    => await taskHandler.SendResult(blobId,
                                    content.ToArray());

  private static async Task CreateNewBlobs(this IEnumerable<TaskNode> taskNodes,
                                           ITaskHandler               taskHandler,
                                           CancellationToken          cancellationToken)
  {
    var enumerableNodes = taskNodes.ToList();
    var nodesWithNewBlobs = enumerableNodes.Where(x => !Equals(x.DataDependenciesContent,
                                                               ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
                                           .ToList();
    if (nodesWithNewBlobs.Any())
    {
      var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);

      var createdBlobDictionary = new Dictionary<string, BlobInfo>();

      await foreach (var blob in taskHandler.CreateBlobs(blobKeyValues,
                                                         cancellationToken))
      {
        createdBlobDictionary[blob.BlobName] = blob;
      }

      foreach (var taskNode in enumerableNodes)
      foreach (var dependency in taskNode.DataDependenciesContent)
      {
        if (createdBlobDictionary.TryGetValue(dependency.Key,
                                              out var createdBlob))
        {
          taskNode.DataDependencies.Add(createdBlob);
        }
      }
    }

    var nodeWithNewPayloads = enumerableNodes.Where(x => Equals(x.Payload,
                                                                null))
                                             .ToList();

    if (nodeWithNewPayloads.Any())
    {
      var payloadBlobKeyValues = nodeWithNewPayloads.Select(x => x.PayloadContent);

      var payloadBlobDictionary = new Dictionary<string, BlobInfo>();

      await foreach (var blob in taskHandler.CreateBlobs(payloadBlobKeyValues,
                                                         cancellationToken))
      {
        payloadBlobDictionary[blob.BlobName] = blob;
      }

      foreach (var taskNode in enumerableNodes)
      {
        if (payloadBlobDictionary.TryGetValue(taskNode.PayloadContent.Key,
                                              out var createdBlob))
        {
          taskNode.Payload = createdBlob;
        }
      }
    }

    var nodeWithNewExpectedOutputs = enumerableNodes.Where(x => x.ExpectedOutputsName.Any())
                                                    .ToList();
    if (nodeWithNewExpectedOutputs.Any())
    {
      var blobKeyValues = nodeWithNewExpectedOutputs.SelectMany(x => x.ExpectedOutputsName);

      var createdBlobDictionary = new Dictionary<string, BlobInfo>();

      await foreach (var blob in taskHandler.CreateMetadataBlobs(blobKeyValues,
                                                                 cancellationToken))
      {
        createdBlobDictionary[blob.BlobName] = blob;
      }

      foreach (var taskNode in enumerableNodes)
      foreach (var dependency in taskNode.DataDependenciesContent)
      {
        if (createdBlobDictionary.TryGetValue(dependency.Key,
                                              out var createdBlob))
        {
          taskNode.ExpectedOutputs.Add(createdBlob);
        }
      }
    }
  }
}
