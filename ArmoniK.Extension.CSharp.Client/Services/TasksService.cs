using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ArmoniK.Api.gRPC.V1.Tasks.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class TasksService : ITasksService
{
    private readonly IBlobService _blobService;
    private readonly ObjectPool<ChannelBase> _channelPool;
    private readonly ILogger<TasksService> _logger;

    public TasksService(ObjectPool<ChannelBase> channel, IBlobService blobService, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<TasksService>();
        _blobService = blobService;
    }

    public async Task<IEnumerable<TaskInfos>> SubmitTasksAsync(string sessionId, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default)
    {
        var enumerableTaskNodes = taskNodes.ToList();

        // Validate each task node
        if (enumerableTaskNodes.Any(node => node.ExpectedOutputs == null || !node.ExpectedOutputs.Any()))
            throw new InvalidOperationException("Expected outputs cannot be empty.");

        await CreateNewBlobs(sessionId, enumerableTaskNodes, cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var taskCreations = enumerableTaskNodes.Select(taskNode => new SubmitTasksRequest.Types.TaskCreation
        {
            PayloadId = taskNode.Payload.Id,
            ExpectedOutputKeys = { taskNode.ExpectedOutputs.Select(i => i.Id) },
            DataDependencies = { taskNode.DataDependencies?.Select(i => i.Id) ?? Enumerable.Empty<string>() },
            TaskOptions = taskNode.TaskOptions?.ToTaskOptions()
        }).ToList();


        var submitTasksRequest = new SubmitTasksRequest
        {
            SessionId = sessionId,
            TaskCreations = { taskCreations.ToList() }
        };

        var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest);

        return taskSubmissionResponse.TaskInfos.Select(x => new TaskInfos(x, sessionId));
    }

    public async Task<TaskState> GetTasksDetailedAsync(string taskId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var tasks = await tasksClient.GetTaskAsync(new GetTaskRequest { TaskId = taskId });

        return new TaskState
        {
            DataDependencies = tasks.Task.DataDependencies,
            ExpectedOutputs = tasks.Task.ExpectedOutputIds,
            TaskId = tasks.Task.Id,
            Status = tasks.Task.Status,
            CreateAt = tasks.Task.CreatedAt.ToDateTime(),
            StartedAt = tasks.Task.StartedAt.ToDateTime(),
            EndedAt = tasks.Task.EndedAt.ToDateTime(),
            SessionId = tasks.Task.SessionId
        };
    }

    public async Task<IEnumerable<TaskState>> ListTasksDetailedAsync(string sessionId, TaskPagination paginationOptions,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var tasks = await tasksClient.ListTasksDetailedAsync(new ListTasksRequest
            {
                Filters = paginationOptions.Filter, // should have sessionId filter... see how it should be implemented
                Page = paginationOptions.Page,
                PageSize = paginationOptions.PageSize,
                Sort = new ListTasksRequest.Types.Sort { Direction = paginationOptions.SortDirection }
            }
        );

        return tasks.Tasks.Select(x => new TaskState
        {
            DataDependencies = x.DataDependencies,
            ExpectedOutputs = x.ExpectedOutputIds,
            TaskId = x.Id,
            Status = x.Status,
            CreateAt = x.CreatedAt.ToDateTime(),
            StartedAt = x.StartedAt.ToDateTime(),
            EndedAt = x.EndedAt.ToDateTime(),
            SessionId = x.SessionId
        });
    }

    private async Task CreateNewBlobs(string sessionId, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken)
    {
        var enumerableNodes = taskNodes.ToList();
        var nodesWithNewBlobs = enumerableNodes
            .Where(x => !Equals(x.DataDependenciesContent, ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
            .ToList();

        if (nodesWithNewBlobs.Any())
        {
            var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);
            var createdBlobs =
                await _blobService.CreateBlobsAsync(sessionId, blobKeyValues,
                    cancellationToken);
            var createdBlobDictionary = createdBlobs.ToDictionary(b => b.Name);

            foreach (var taskNode in enumerableNodes)
            foreach (var dependency in taskNode.DataDependenciesContent)
                if (createdBlobDictionary.TryGetValue(dependency.Key, out var createdBlob))
                    taskNode.DataDependencies.Add(createdBlob);
        }

        var nodeWithNewPayloads = enumerableNodes
            .Where(x => Equals(x.Payload, null))
            .ToList();

        if (nodeWithNewPayloads.Any())
        {
            var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);
            var createdBlobs =
                await _blobService.CreateBlobsAsync(sessionId, blobKeyValues,
                    cancellationToken);
            var createdBlobDictionary = createdBlobs.ToDictionary(b => b.Name);

            foreach (var taskNode in enumerableNodes)
                if (createdBlobDictionary.TryGetValue(taskNode.PayloadContent.Key, out var createdBlob))
                    taskNode.Payload = createdBlob;
        }
    }
}