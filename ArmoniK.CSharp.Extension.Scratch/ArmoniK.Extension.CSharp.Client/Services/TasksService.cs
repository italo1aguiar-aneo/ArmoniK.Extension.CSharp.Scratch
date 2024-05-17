using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
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

    public async Task<IEnumerable<string>> SubmitTasksAsync(IEnumerable<TaskNode> taskNodes, Session session,
        CancellationToken cancellationToken = default)
    {
        // À voir si ça doit etre fait ici

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var newBlobs = taskNodes
            .Where(x => !Equals(x.DataDependenciesContent, ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
            .ToList();

        if (newBlobs.Any())
        {
            var blobNames = newBlobs.SelectMany(x => x.DataDependenciesContent.Select(y => y.Key));
            var blobKeyValues = newBlobs.SelectMany(x => x.DataDependenciesContent);
            var createdBlobs =
                await _blobService.CreateBlobsAsync(blobNames, blobKeyValues, session, cancellationToken);
            var createdBlobDictionary = createdBlobs.ToDictionary(b => b.Name);

            foreach (var taskNode in taskNodes)
            foreach (var dependency in taskNode.DataDependenciesContent)
                if (createdBlobDictionary.TryGetValue(dependency.Key, out var createdBlob))
                    taskNode.DataDependencies.Add(createdBlob);
        }

        // until here

        var taskCreations = new ConcurrentBag<SubmitTasksRequest.Types.TaskCreation>();

        Parallel.ForEach(taskNodes, taskNode =>
        {
            var taskCreation = new SubmitTasksRequest.Types.TaskCreation
            {
                PayloadId = taskNode.Payload.BlobId,
                ExpectedOutputKeys = { taskNode.ExpectedOutputs.Select(i => i.BlobId) },
                DataDependencies = { taskNode.DataDependencies?.Select(i => i.BlobId) ?? [] },
                TaskOptions = taskNode.TaskOptions
            };
            taskCreations.Add(taskCreation);
        });

        var submitTasksRequest = new SubmitTasksRequest
        {
            SessionId = session.Id,
            TaskCreations = { taskCreations.ToList() }
        };

        var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest);

        return taskSubmissionResponse.TaskInfos.Select(i => i.TaskId);
    }
}