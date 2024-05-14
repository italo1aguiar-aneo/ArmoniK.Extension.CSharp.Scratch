using ArmoniK.Api.Client.Options;
using ArmoniK.Api.Client.Submitter;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ArmoniK.Api.Client;

namespace ArmoniK.Extension.CSharp.Client
{
    public class SessionManager
    {
        private readonly ChannelBase _channel;
        private readonly Results.ResultsClient _resultsClient;
        private readonly Sessions.SessionsClient _sessionClient;
        private readonly Tasks.TasksClient _tasksClient;

        private CreateSessionReply _createSessionReply;
        private Events.EventsClient _eventsClient;
        private string _sessionId;

        private SessionTaskNode _sessionTasksGraph;
        private readonly TaskOptions _taskOptions;
        private Dictionary<string, IEnumerable<string>> _taskResultMap;
        private readonly IEnumerable<string> _partitionIds;

        public SessionManager(string endpoint, IEnumerable<string> partitions,
            TaskOptions taskOptions)
        {
            _channel = GrpcChannelFactory.CreateChannel(new GrpcClient
            {
                Endpoint = endpoint
            });

            _tasksClient = new Tasks.TasksClient(_channel);
            _resultsClient = new Results.ResultsClient(_channel);
            _sessionClient = new Sessions.SessionsClient(_channel);
            _eventsClient = new Events.EventsClient(_channel);
            _taskOptions = taskOptions;
            _partitionIds = partitions;
        }

        public void StartSession()
        {
            _createSessionReply = _sessionClient.CreateSession(new CreateSessionRequest
            {
                DefaultTaskOption = _taskOptions,
                PartitionIds =
            {
                _partitionIds
            }
            });
            _sessionId = _createSessionReply.SessionId;
        }

        public TaskCreationResponse SendTask(string partitionId, ByteString payload, string[]? expectedOutputs = null,
            string[]? dataDependencies = null)
        {
            expectedOutputs ??= ["Result"];
            _sessionTasksGraph ??= new SessionTaskNode(_tasksClient, _resultsClient, _sessionId);

            var taskCreationResponse
                = _sessionTasksGraph.SubmitTask(payload, partitionId, expectedOutputs, dataDependencies);

            return taskCreationResponse;
        }

        public async Task LoadDataDependencies(IEnumerable<Blob> dataDependencies)
        {
            try
            {
                await _sessionTasksGraph.UploadDataDependencies(dataDependencies);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task WaitForResultsAsync(ICollection<string> resultIds, CancellationToken cancellationToken = default)
        {
            await _eventsClient.WaitForResultsAsync(_sessionId,
                resultIds,
                cancellationToken);
        }

        public async Task<byte[]> DownloadResult(string resultId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _resultsClient.DownloadResultData(_sessionId, resultId, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
