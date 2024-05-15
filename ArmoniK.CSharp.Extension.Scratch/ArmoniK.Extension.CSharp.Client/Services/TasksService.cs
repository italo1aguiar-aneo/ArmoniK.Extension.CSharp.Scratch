using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Abstracts;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Microsoft.Extensions.Logging;
using static ArmoniK.Api.gRPC.V1.Sessions.Sessions;
using static ArmoniK.Api.gRPC.V1.Tasks.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Services
{
    public class TasksService : BaseArmoniKClientService<TasksService>, ITaskService
    {

        public TasksService(Properties properties, ILoggerFactory loggerFactory, TaskOptions taskOptions = null) : base(
            properties, loggerFactory, taskOptions ?? ClientServiceConnector.InitializeDefaultTaskOptions())
        {
        }

        private Tasks.TasksClient _tasksClient;
        private readonly object _lock = new();

        // Property to access the TasksClient
        private async Task<Tasks.TasksClient> GetTasksClientAsync()
        {
            if (_tasksClient == null)
            {
                await InitializeTasksClientAsync();
            }

            return _tasksClient;
        }

        // Method to initialize the TasksClient
        private async Task InitializeTasksClientAsync()
        {
            if (_tasksClient == null)
            {
                var client = new Tasks.TasksClient(await ChannelPool.GetAsync());
                lock (_lock)
                {
                    _tasksClient ??= client;
                }
            }
        }

        public async Task<IEnumerable<string>> SubmitTasksAsync(IEnumerable<TaskNode> taskNodes, Session session)
        {
            var taskCreations = new ConcurrentBag<SubmitTasksRequest.Types.TaskCreation>();

            Parallel.ForEach(taskNodes, taskNode =>
            {
                var taskCreation = new SubmitTasksRequest.Types.TaskCreation
                {
                    PayloadId = taskNode.Payload.BlobId,
                    ExpectedOutputKeys = { taskNode.ExpectedOutputs.Select(i => i.BlobId) },
                    DataDependencies = { taskNode.DataDependencies?.Select(i => i.BlobId) ?? new List<string>() },
                    TaskOptions = taskNode.TaskOptions ?? TaskOptions,
                };
                taskCreations.Add(taskCreation);
            });

            var submitTasksRequest = new SubmitTasksRequest
            {
                SessionId = session.Id,
                TaskCreations = { taskCreations.ToList() },
                TaskOptions = taskNodes.Single().TaskOptions
            };
            var tasksClient = await GetTasksClientAsync();
            var taskSubmissionResponse = await tasksClient.SubmitTasksAsync(submitTasksRequest);
            return taskSubmissionResponse.TaskInfos.Select(i => i.TaskId);
        }
    }
}