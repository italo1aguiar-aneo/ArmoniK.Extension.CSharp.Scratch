using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.SortDirection;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ArmoniK.Api.gRPC.V1.Tasks.Tasks;
using TaskStatus = ArmoniK.Extension.CSharp.Client.Common.Domain.Task.TaskStatus;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class TasksService : ITasksService
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

    public async Task<IEnumerable<TaskInfos>> SubmitTasksAsync(SessionInfo session, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default)
    {
        var enumerableTaskNodes = taskNodes.ToList();

        // Validate each task node
        if (enumerableTaskNodes.Any(node => node.ExpectedOutputs == null || !node.ExpectedOutputs.Any()))
            throw new InvalidOperationException("Expected outputs cannot be empty.");

        await CreateNewBlobs(session, enumerableTaskNodes, cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var taskCreations = enumerableTaskNodes.Select(taskNode => new SubmitTasksRequest.Types.TaskCreation
        {
            PayloadId = taskNode.Payload.BlobId,
            ExpectedOutputKeys = { taskNode.ExpectedOutputs.Select(i => i.BlobId) },
            DataDependencies = { taskNode.DataDependencies?.Select(i => i.BlobId) ?? Enumerable.Empty<string>() },
            TaskOptions = taskNode.TaskOptions?.ToTaskOptions()
        }).ToList();


        var submitTasksRequest = new SubmitTasksRequest
        {
            SessionId = session.SessionId,
            TaskCreations = { taskCreations.ToList() }
        };

        var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest,
            cancellationToken: cancellationToken);

        return taskSubmissionResponse.TaskInfos.Select(x => new TaskInfos(x, session.SessionId));
    }

    public async IAsyncEnumerable<TaskPage> ListTasksAsync(TaskPagination paginationOptions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var tasks = await tasksClient.ListTasksAsync(new ListTasksRequest
            {
                Filters = paginationOptions.Filter,
                Page = paginationOptions.Page,
                PageSize = paginationOptions.PageSize,
                Sort = new ListTasksRequest.Types.Sort { Direction = (SortDirection)paginationOptions.SortDirection }
            },
            cancellationToken: cancellationToken
        );

        foreach (var task in tasks.Tasks)
            yield return new TaskPage
            {
                TaskId = task.Id,
                TaskStatus = (TaskStatus)task.Status,
                TotalPages = tasks.Total
            };
        ;
    }


    public async Task<TaskState> GetTasksDetailedAsync(string taskId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var tasks = await tasksClient.GetTaskAsync(new GetTaskRequest { TaskId = taskId },
            cancellationToken: cancellationToken);

        return new TaskState
        {
            DataDependencies = tasks.Task.DataDependencies,
            ExpectedOutputs = tasks.Task.ExpectedOutputIds,
            TaskId = tasks.Task.Id,
            Status = (TaskStatus)tasks.Task.Status,
            CreateAt = tasks.Task.CreatedAt.ToDateTime(),
            StartedAt = tasks.Task.StartedAt.ToDateTime(),
            EndedAt = tasks.Task.EndedAt.ToDateTime(),
            SessionId = tasks.Task.SessionId
        };
    }

    public async IAsyncEnumerable<TaskDetailedPage> ListTasksDetailedAsync(SessionInfo session,
        TaskPagination paginationOptions,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var tasks = await tasksClient.ListTasksDetailedAsync(new ListTasksRequest
            {
                Filters = paginationOptions.Filter,
                Page = paginationOptions.Page,
                PageSize = paginationOptions.PageSize,
                Sort = new ListTasksRequest.Types.Sort { Direction = (SortDirection)paginationOptions.SortDirection }
            },
            cancellationToken: cancellationToken
        );

        foreach (var task in tasks.Tasks)
            yield return new TaskDetailedPage
            {
                TaskDetails = new TaskState
                {
                    DataDependencies = task.DataDependencies,
                    ExpectedOutputs = task.ExpectedOutputIds,
                    TaskId = task.Id,
                    Status = (TaskStatus)task.Status,
                    CreateAt = task.CreatedAt.ToDateTime(),
                    StartedAt = task.StartedAt.ToDateTime(),
                    EndedAt = task.EndedAt.ToDateTime(),
                    SessionId = task.SessionId
                },
                TotalPages = tasks.Total
            };
        ;
    }

    public async Task CancelTask(IEnumerable<string> taskIds, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        await tasksClient.CancelTasksAsync(new CancelTasksRequest
        {
            TaskIds = { taskIds }
        });
    }

    private async Task CreateNewBlobs(SessionInfo session, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken)
    {
        var enumerableNodes = taskNodes.ToList();
        var nodesWithNewBlobs = enumerableNodes
            .Where(x => !Equals(x.DataDependenciesContent, ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
            .ToList();

        if (nodesWithNewBlobs.Any())
        {
            var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);

            var createdBlobDictionary = new Dictionary<string, BlobInfo>();

            await foreach (var blob in _blobService.CreateBlobsAsync(session, blobKeyValues, cancellationToken))
                createdBlobDictionary[blob.BlobName] = blob;

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
            var payloadBlobKeyValues = nodeWithNewPayloads.Select(x => x.PayloadContent);

            var payloadBlobDictionary = new Dictionary<string, BlobInfo>();

            await foreach (var blob in _blobService.CreateBlobsAsync(session, payloadBlobKeyValues, cancellationToken))
                payloadBlobDictionary[blob.BlobName] = blob;

            foreach (var taskNode in enumerableNodes)
                if (payloadBlobDictionary.TryGetValue(taskNode.PayloadContent.Key, out var createdBlob))
                    taskNode.Payload = createdBlob;
        }
    }
}