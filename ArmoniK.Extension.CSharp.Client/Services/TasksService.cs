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
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using static ArmoniK.Api.gRPC.V1.Tasks.Tasks;

using TaskStatus = ArmoniK.Extension.CSharp.Client.Common.Domain.Task.TaskStatus;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class TasksService : ITasksService
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

    var tasksClient = new TasksClient(channel);

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

    var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest);

    return taskSubmissionResponse.TaskInfos.Select(x => new TaskInfos(x,
                                                                      session.SessionId));
  }

  public async Task<TaskState> GetTasksDetailedAsync(string            taskId,
                                                     CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new TasksClient(channel);

    var tasks = await tasksClient.GetTaskAsync(new GetTaskRequest
                                               {
                                                 TaskId = taskId,
                                               });

    return new TaskState
           {
             DataDependencies = tasks.Task.DataDependencies,
             ExpectedOutputs  = tasks.Task.ExpectedOutputIds,
             TaskId           = tasks.Task.Id,
             Status           = (TaskStatus?)tasks.Task.Status,
             CreateAt         = tasks.Task.CreatedAt.ToDateTime(),
             StartedAt        = tasks.Task.StartedAt.ToDateTime(),
             EndedAt          = tasks.Task.EndedAt.ToDateTime(),
             SessionId        = tasks.Task.SessionId,
           };
  }

  public async Task<IEnumerable<TaskState>> ListTasksDetailedAsync(SessionInfo       session,
                                                                   TaskPagination    paginationOptions,
                                                                   CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);

    var tasksClient = new TasksClient(channel);

    var tasks = await tasksClient.ListTasksDetailedAsync(new ListTasksRequest
                                                         {
                                                           Filters  = paginationOptions.Filter, // should have sessionId filter... see how it should be implemented
                                                           Page     = paginationOptions.Page,
                                                           PageSize = paginationOptions.PageSize,
                                                           Sort = new ListTasksRequest.Types.Sort
                                                                  {
                                                                    Direction = paginationOptions.SortDirection,
                                                                  },
                                                         });

    return tasks.Tasks.Select(x => new TaskState
                                   {
                                     DataDependencies = x.DataDependencies,
                                     ExpectedOutputs  = x.ExpectedOutputIds,
                                     TaskId           = x.Id,
                                     Status           = (TaskStatus?)x.Status,
                                     CreateAt         = x.CreatedAt.ToDateTime(),
                                     StartedAt        = x.StartedAt.ToDateTime(),
                                     EndedAt          = x.EndedAt.ToDateTime(),
                                     SessionId        = x.SessionId,
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
      var createdBlobs = await blobService_.CreateBlobsAsync(session,
                                                             blobKeyValues,
                                                             cancellationToken);
      var createdBlobDictionary = createdBlobs.ToDictionary(b => b.BlobName);

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
      var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);
      var createdBlobs = await blobService_.CreateBlobsAsync(session,
                                                             blobKeyValues,
                                                             cancellationToken);
      var createdBlobDictionary = createdBlobs.ToDictionary(b => b.BlobName);

      foreach (var taskNode in enumerableNodes)
      {
        if (createdBlobDictionary.TryGetValue(taskNode.PayloadContent.Key,
                                              out var createdBlob))
        {
          taskNode.Payload = createdBlob;
        }
      }
    }
  }
}
