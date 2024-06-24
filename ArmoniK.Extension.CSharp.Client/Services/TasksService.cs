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
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Enum;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using TaskStatus = ArmoniK.Extension.CSharp.Client.Common.Domain.Task.TaskStatus;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class TasksService : ITasksService
{
  private readonly IBlobService            blobService_;
  private readonly ObjectPool<ChannelBase> channelPool_;
  private readonly ILogger<TasksService>   logger_;

  public TasksService(ObjectPool<ChannelBase> channel,
                      IBlobService            blobService,
                      ILoggerFactory          loggerFactory)
  {
    channelPool_ = channel;
    logger_      = loggerFactory.CreateLogger<TasksService>();
    blobService_ = blobService;
  }

  public async Task<IEnumerable<TaskInfos>> SubmitTasksAsync(SessionInfo           session,
                                                             IEnumerable<TaskNode> taskNodes,
                                                             CancellationToken     cancellationToken = default)
  {
    var enumerableTaskNodes = taskNodes.ToList();
    // Validate each task node
    if (enumerableTaskNodes.Any(node => node.ExpectedOutputs == null || !node.ExpectedOutputs.Any()))
    {
      throw new InvalidOperationException("Expected outputs cannot be empty.");
    }

    await CreateNewBlobs(session,
                         enumerableTaskNodes,
                         cancellationToken);
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var tasksClient = new Tasks.TasksClient(channel);
    var taskCreations = enumerableTaskNodes.Select(taskNode => new SubmitTasksRequest.Types.TaskCreation
                                                               {
                                                                 PayloadId = taskNode.Payload.BlobId,
                                                                 ExpectedOutputKeys =
                                                                 {
                                                                   taskNode.ExpectedOutputs.Select(i => i.BlobId),
                                                                 },
                                                                 DataDependencies =
                                                                 {
                                                                   taskNode.DataDependencies?.Select(i => i.BlobId) ?? Enumerable.Empty<string>(),
                                                                 },
                                                                 TaskOptions = taskNode.TaskOptions?.ToTaskOptions(),
                                                               })
                                           .ToList();
    var submitTasksRequest = new SubmitTasksRequest
                             {
                               SessionId = session.SessionId,
                               TaskCreations =
                               {
                                 taskCreations.ToList(),
                               },
                             };

    var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest,
                                                                    cancellationToken: cancellationToken);

    return taskSubmissionResponse.TaskInfos.Select(x => new TaskInfos(x,
                                                                      session.SessionId));
  }

  public async IAsyncEnumerable<TaskPage> ListTasksAsync(TaskPagination                             paginationOptions,
                                                         [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new Tasks.TasksClient(channel);

    var tasks = await tasksClient.ListTasksAsync(new ListTasksRequest
                                                 {
                                                   Filters  = paginationOptions.Filter,
                                                   Page     = paginationOptions.Page,
                                                   PageSize = paginationOptions.PageSize,
                                                   Sort = new ListTasksRequest.Types.Sort
                                                          {
                                                            Direction = paginationOptions.SortDirection.ToGrpc(),
                                                          },
                                                 },
                                                 cancellationToken: cancellationToken);
    yield return new TaskPage
                 {
                   TotalTasks = tasks.Total,
                   TasksData = tasks.Tasks.Select(x => new Tuple<string, TaskStatus>(x.Id,
                                                                                     x.Status.ToInternalStatus())),
                 };
  }


  public async Task<TaskState> GetTasksDetailedAsync(string            taskId,
                                                     CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new Tasks.TasksClient(channel);

    var tasks = await tasksClient.GetTaskAsync(new GetTaskRequest
                                               {
                                                 TaskId = taskId,
                                               },
                                               cancellationToken: cancellationToken);

    return new TaskState
           {
             DataDependencies = tasks.Task.DataDependencies,
             ExpectedOutputs  = tasks.Task.ExpectedOutputIds,
             TaskId           = tasks.Task.Id,
             Status           = tasks.Task.Status.ToInternalStatus(),
             CreateAt         = tasks.Task.CreatedAt.ToDateTime(),
             StartedAt        = tasks.Task.StartedAt.ToDateTime(),
             EndedAt          = tasks.Task.EndedAt.ToDateTime(),
             SessionId        = tasks.Task.SessionId,
           };
  }

  public async IAsyncEnumerable<TaskDetailedPage> ListTasksDetailedAsync(SessionInfo                                session,
                                                                         TaskPagination                             paginationOptions,
                                                                         [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new Tasks.TasksClient(channel);

    var tasks = await tasksClient.ListTasksDetailedAsync(new ListTasksRequest
                                                         {
                                                           Filters  = paginationOptions.Filter,
                                                           Page     = paginationOptions.Page,
                                                           PageSize = paginationOptions.PageSize,
                                                           Sort = new ListTasksRequest.Types.Sort
                                                                  {
                                                                    Direction = paginationOptions.SortDirection.ToGrpc(),
                                                                  },
                                                         },
                                                         cancellationToken: cancellationToken);

    yield return new TaskDetailedPage
                 {
                   TaskDetails = tasks.Tasks.Select(task => new TaskState
                                                            {
                                                              DataDependencies = task.DataDependencies,
                                                              ExpectedOutputs  = task.ExpectedOutputIds,
                                                              TaskId           = task.Id,
                                                              Status           = task.Status.ToInternalStatus(),
                                                              CreateAt         = task.CreatedAt.ToDateTime(),
                                                              StartedAt        = task.StartedAt.ToDateTime(),
                                                              EndedAt          = task.EndedAt.ToDateTime(),
                                                              SessionId        = task.SessionId,
                                                            }),
                   TotalTasks = tasks.Total,
                 };
  }

  public async Task CancelTask(IEnumerable<string> taskIds,
                               CancellationToken   cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new Tasks.TasksClient(channel);

    await tasksClient.CancelTasksAsync(new CancelTasksRequest
                                       {
                                         TaskIds =
                                         {
                                           taskIds,
                                         },
                                       });
  }

  private async Task CreateNewBlobs(SessionInfo           session,
                                    IEnumerable<TaskNode> taskNodes,
                                    CancellationToken     cancellationToken)
  {
    var enumerableNodes = taskNodes.ToList();
    var nodesWithNewBlobs = enumerableNodes.Where(x => !Equals(x.DataDependenciesContent,
                                                               ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
                                           .ToList();
    if (nodesWithNewBlobs.Any())
    {
      var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);

      var createdBlobDictionary = new Dictionary<string, BlobInfo>();

      await foreach (var blob in blobService_.CreateBlobsAsync(session,
                                                               blobKeyValues,
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

      await foreach (var blob in blobService_.CreateBlobsAsync(session,
                                                               payloadBlobKeyValues,
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
  }
}
