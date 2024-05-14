using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Tasks;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArmoniK.Extension.CSharp.Client
{
    public class SessionTaskNode
    {
        private readonly Results.ResultsClient _resultsClient;
        private readonly string _sessionId;
        private readonly Tasks.TasksClient _tasksClient;
        private CreateResultsMetaDataResponse? _dataDependencies;
        private CreateResultsMetaDataResponse _expectedOutputs;
        private CreateResultsResponse _payload;
        private SubmitTasksResponse _taskSubmissionResponse;

        public SessionTaskNode(Tasks.TasksClient tasksClient, Results.ResultsClient resultsClient, string sessionId)
        {
            _tasksClient = tasksClient;
            _resultsClient = resultsClient;
            _sessionId = sessionId;
        }

        public TaskCreationResponse SubmitTask(ByteString payload, string partitionId, string[] expectedOutputs,
            string[]? dataDependencies = null)
        {
            _payload = _resultsClient.CreateResults(new CreateResultsRequest
            {
                SessionId = _sessionId,
                Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Data = payload,
                        Name = "Payload"
                    }
                }
            });

            _expectedOutputs =
                _resultsClient.CreateResultsMetaData(new CreateResultsMetaDataRequest
                {
                    SessionId = _sessionId,
                    Results =
                    {
                        Enumerable.Range(0, expectedOutputs.Length).Select(i =>
                            new CreateResultsMetaDataRequest.Types.ResultCreate
                            {
                                Name = expectedOutputs[i]
                            })
                    }
                });

            _dataDependencies = dataDependencies is not null
                ? _resultsClient.CreateResultsMetaData(new CreateResultsMetaDataRequest
                {
                    SessionId = _sessionId,
                    Results =
                    {
                        Enumerable.Range(0, dataDependencies.Length).Select(i =>
                            new CreateResultsMetaDataRequest.Types.ResultCreate
                            {
                                Name = dataDependencies[i]
                            })
                    }
                })
                : null;

            var taskCreation = new SubmitTasksRequest.Types.TaskCreation
            {
                PayloadId = _payload.Results.Single().ResultId,
                ExpectedOutputKeys =
                {
                    _expectedOutputs.Results.Select(result => result.ResultId)
                },
                DataDependencies =
                {
                    _dataDependencies?.Results.Select(result => result.ResultId).ToList() ?? []
                }
            };

            _taskSubmissionResponse = _tasksClient.SubmitTasks(new SubmitTasksRequest
            {
                SessionId = _sessionId,
                TaskCreations =
                {
                    taskCreation
                }
            });

            return new TaskCreationResponse
            (
                _dataDependencies?.Results.Select(result =>
                        new KeyValuePair<string, Blob>(result.Name,
                            new Blob(result.ResultId,
                                result.Name)))
                    .ToDictionary(pair => pair.Key, pair => pair.Value) ??
                Array.Empty<KeyValuePair<string, Blob>>().ToDictionary(pair => pair.Key, pair => pair.Value),
                _expectedOutputs.Results.Select(result =>
                        new KeyValuePair<string, string>(result.Name, result.ResultId))
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
            );
        }

        public async Task UploadDataDependencies(IEnumerable<Blob> dataDependencies)
        {
            var uploadStream = _resultsClient.UploadResultData();
            try
            {
                foreach (var dataDependency in dataDependencies)
                    await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                    {
                        DataChunk = dataDependency.Content,
                        Id = new UploadResultDataRequest.Types.ResultIdentifier
                        {
                            ResultId = dataDependency.BlobId,
                            SessionId = _sessionId
                        }
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await uploadStream.RequestStream.CompleteAsync();
            }
        }
    }
}
