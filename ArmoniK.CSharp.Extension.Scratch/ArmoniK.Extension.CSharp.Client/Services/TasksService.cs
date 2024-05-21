using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Exceptions;
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

    private Session _session;

    public TasksService(ObjectPool<ChannelBase> channel, IBlobService blobService, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<TasksService>();
        _blobService = blobService;
    }

    public void SetSession(Session session)
    {
        _session = session;
    }

    public async Task<IEnumerable<string>> SubmitTasksAsync(IEnumerable<TaskNode> taskNodes, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null) throw new UnsetSessionException();

        // Choix de desgin à faire

        await CreateNewBlobs(taskNodes, session, cancellationToken);

        // Choix de desgin à faire

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);

        var tasksClient = new TasksClient(channel);

        var taskCreations = taskNodes.Select(taskNode => new SubmitTasksRequest.Types.TaskCreation
        {
            PayloadId = taskNode.Payload.Id,
            ExpectedOutputKeys = { taskNode.ExpectedOutputs.Select(i => i.Id) },
            DataDependencies = { taskNode.DataDependencies?.Select(i => i.Id) ?? Enumerable.Empty<string>() },
            TaskOptions = taskNode.TaskOptions
        }).ToList();


        var submitTasksRequest = new SubmitTasksRequest
        {
            SessionId = session.Id,
            TaskCreations = { taskCreations.ToList() }
        };

        var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest);

        return taskSubmissionResponse.TaskInfos.Select(i => i.TaskId);
    }

    private async Task CreateNewBlobs(IEnumerable<TaskNode> taskNodes, Session session,
        CancellationToken cancellationToken)
    {
        var nodesWithNewBlobs = taskNodes
            .Where(x => !Equals(x.DataDependenciesContent, ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty))
            .ToList();


        if (nodesWithNewBlobs.Any())
        {
            var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);
            var createdBlobs =
                await _blobService.CreateBlobsAsync(blobKeyValues, session,
                    cancellationToken);
            var createdBlobDictionary = createdBlobs.ToDictionary(b => b.Name);

            foreach (var taskNode in taskNodes)
            foreach (var dependency in taskNode.DataDependenciesContent)
                if (createdBlobDictionary.TryGetValue(dependency.Key, out var createdBlob))
                    taskNode.DataDependencies.Add(createdBlob);
        }

        var nodeWithNewPayloads = taskNodes
            .Where(x => !Equals(x.Payload, null))
            .ToList();

        if (nodeWithNewPayloads.Any())
        {
            var blobKeyValues = nodesWithNewBlobs.SelectMany(x => x.DataDependenciesContent);
            var createdBlobs =
                await _blobService.CreateBlobsAsync(blobKeyValues, session,
                    cancellationToken);
            var createdBlobDictionary = createdBlobs.ToDictionary(b => b.Name);

            foreach (var taskNode in taskNodes)
                if (createdBlobDictionary.TryGetValue(taskNode.PayloadContent.Key, out var createdBlob))
                    taskNode.Payload = createdBlob;
        }
    }
}