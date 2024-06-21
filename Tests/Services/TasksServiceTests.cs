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

using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests.Services;

public class TasksServiceTests
{
  [Test]
  public async Task CreateTask_ReturnsNewTaskWithId()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    var mockCallInvoker = new Mock<CallInvoker>();

    var submitTaskResponse = Task.FromResult(new SubmitTasksResponse
                                             {
                                               TaskInfos =
                                               {
                                                 new SubmitTasksResponse.Types.TaskInfo
                                                 {
                                                   TaskId = "taskId1",
                                                   ExpectedOutputIds =
                                                   {
                                                     new List<string>
                                                     {
                                                       "blobId1",
                                                     },
                                                   },
                                                   PayloadId = "payloadId1",
                                                 },
                                               },
                                             });

    // Configure SubmitTask call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<SubmitTasksRequest>()))
                   .Returns(new AsyncUnaryCall<SubmitTasksResponse>(submitTaskResponse,
                                                                    Task.FromResult(new Metadata()),
                                                                    () => Status.DefaultSuccess,
                                                                    () => new Metadata(),
                                                                    () =>
                                                                    {
                                                                    }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var taskNodes = new List<TaskNode>
                    {
                      new TaskNode
                      {
                        ExpectedOutputs = new List<BlobInfo>
                                          {
                                            new BlobInfo
                                            {
                                              BlobName  = "blob1",
                                              BlobId    = "blobId1",
                                              SessionId = "sessionId1",
                                            },
                                          },
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payload1",
                                    BlobId    = "payloadId1",
                                    SessionId = "sessionId1",
                                  },
                      },
                    };

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var mockBlobService = new Mock<IBlobService>().Object;

    var taskService = TasksServiceFactory.CreateTaskService(objectPool,
                                                            mockBlobService,
                                                            NullLoggerFactory.Instance);
    // Act
    var result = await taskService.SubmitTasksAsync(new SessionInfo("sessionId1"),
                                                    taskNodes);

    // Assert
    ClassicAssert.AreEqual("taskId1",
                           result.FirstOrDefault()
                                 ?.TaskId);
    ClassicAssert.AreEqual("payloadId1",
                           result.FirstOrDefault()
                                 ?.PayloadId);
    ClassicAssert.AreEqual("blobId1",
                           result.FirstOrDefault()
                                 .ExpectedOutputs.First());
  }

  [Test]
  public async Task SubmitTasksAsync_MultipleTasksWithOutputs_ReturnsCorrectResponses()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };
    var mockCallInvoker = new Mock<CallInvoker>();
    var taskResponse = new SubmitTasksResponse
                       {
                         TaskInfos =
                         {
                           new SubmitTasksResponse.Types.TaskInfo
                           {
                             TaskId    = "taskId1",
                             PayloadId = "payloadId1",
                             ExpectedOutputIds =
                             {
                               "outputId1",
                             },
                           },
                           new SubmitTasksResponse.Types.TaskInfo
                           {
                             TaskId    = "taskId2",
                             PayloadId = "payloadId2",
                             ExpectedOutputIds =
                             {
                               "outputId2",
                             },
                           },
                         },
                       };

    // Configure SubmitTask call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<SubmitTasksRequest>()))
                   .Returns(new AsyncUnaryCall<SubmitTasksResponse>(Task.FromResult(taskResponse),
                                                                    Task.FromResult(new Metadata()),
                                                                    () => Status.DefaultSuccess,
                                                                    () => new Metadata(),
                                                                    () =>
                                                                    {
                                                                    }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool      = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
    var mockBlobService = new Mock<IBlobService>().Object;

    var taskService = TasksServiceFactory.CreateTaskService(objectPool,
                                                            mockBlobService,
                                                            NullLoggerFactory.Instance);

    var taskNodes = new List<TaskNode>
                    {
                      new TaskNode
                      {
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payload1",
                                    BlobId    = "payloadId1",
                                    SessionId = "sessionId1",
                                  },
                        ExpectedOutputs = new List<BlobInfo>
                                          {
                                            new BlobInfo
                                            {
                                              BlobName  = "output1",
                                              BlobId    = "outputId1",
                                              SessionId = "sessionId1",
                                            },
                                          },
                      },
                      new TaskNode
                      {
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payload2",
                                    BlobId    = "payloadId2",
                                    SessionId = "sessionId1",
                                  },
                        ExpectedOutputs = new List<BlobInfo>
                                          {
                                            new BlobInfo
                                            {
                                              BlobName  = "output2",
                                              BlobId    = "outputId2",
                                              SessionId = "sessionId1",
                                            },
                                          },
                      },
                    };

    // Act
    var result = await taskService.SubmitTasksAsync(new SessionInfo("sessionId1"),
                                                    taskNodes);

    // Assert
    ClassicAssert.AreEqual(2,
                           result.Count());

    Assert.That(result,
                Has.Some.Matches<TaskInfos>(r => r.TaskId == "taskId1" && r.PayloadId == "payloadId1" && r.ExpectedOutputs.Contains("outputId1")),
                "Result should contain an item with taskId1, payloadId1, and outputId1");

    Assert.That(result,
                Has.Some.Matches<TaskInfos>(r => r.TaskId == "taskId2" && r.PayloadId == "payloadId2" && r.ExpectedOutputs.Contains("outputId2")),
                "Result should contain an item with taskId2, payloadId2, and outputId2");
  }


  [Test]
  public void SubmitTasksAsync_WithEmptyExpectedOutputs_ThrowsException()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
    {
      CallBase = true,
    };
    var mockCallInvoker = new Mock<CallInvoker>();

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
    var mockBlobService = new Mock<IBlobService>().Object;

    var taskService = TasksServiceFactory.CreateTaskService(objectPool,
                                                            mockBlobService,
                                                            NullLoggerFactory.Instance);

    var taskNodes = new List<TaskNode>
                    {
                      new TaskNode
                      {
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payload1",
                                    BlobId    = "payloadId1",
                                    SessionId = "sessionId1",
                                  },
                        ExpectedOutputs = new List<BlobInfo>(), // Empty expected outputs
                      },
                    };

    // Act & Assert
    Assert.ThrowsAsync<InvalidOperationException>(() => taskService.SubmitTasksAsync(new SessionInfo("sessionId1"),
                                                                                     taskNodes));
  }

  [Test]
  public async Task SubmitTasksAsync_WithDataDependencies_CreatesBlobsCorrectly()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };
    var mockCallInvoker = new Mock<CallInvoker>();
    var taskResponse = new SubmitTasksResponse
                       {
                         TaskInfos =
                         {
                           new SubmitTasksResponse.Types.TaskInfo
                           {
                             TaskId    = "taskId1",
                             PayloadId = "payloadId1",
                             ExpectedOutputIds =
                             {
                               "outputId1",
                             },
                             DataDependencies =
                             {
                               "dependencyBlobId",
                             },
                           },
                         },
                       };
    // Configure SubmitTask call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<SubmitTasksRequest>()))
                   .Returns(new AsyncUnaryCall<SubmitTasksResponse>(Task.FromResult(taskResponse),
                                                                    Task.FromResult(new Metadata()),
                                                                    () => Status.DefaultSuccess,
                                                                    () => new Metadata(),
                                                                    () =>
                                                                    {
                                                                    }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool      = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
    var mockBlobService = new Mock<IBlobService>();

    var expectedBlobs = new List<BlobInfo>
                        {
                          new BlobInfo
                          {
                            BlobName  = "dependencyBlob",
                            BlobId    = "dependencyBlobId",
                            SessionId = "sessionId1",
                          },
                        };

    mockBlobService.Setup(m => m.CreateBlobsAsync(It.IsAny<SessionInfo>(),
                                                  It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                                                  It.IsAny<CancellationToken>()))
                   .ReturnsAsync(expectedBlobs);

    var taskService = TasksServiceFactory.CreateTaskService(objectPool,
                                                            mockBlobService.Object,
                                                            NullLoggerFactory.Instance);

    var dataDependenciesContent = new Dictionary<string, ReadOnlyMemory<byte>>
                                  {
                                    {
                                      "dependencyBlob", new ReadOnlyMemory<byte>(new byte[]
                                                                                 {
                                                                                   1,
                                                                                   2,
                                                                                   3,
                                                                                 })
                                    },
                                  };

    var taskNodes = new List<TaskNode>
                    {
                      new TaskNode
                      {
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payloadId",
                                    BlobId    = "blobId",
                                    SessionId = "sessionId1",
                                  },
                        ExpectedOutputs = new List<BlobInfo>
                                          {
                                            new BlobInfo
                                            {
                                              BlobName  = "output1",
                                              BlobId    = "outputId1",
                                              SessionId = "sessionId1",
                                            },
                                          },
                        DataDependenciesContent = dataDependenciesContent,
                      },
                    };

    // Act
    var result = await taskService.SubmitTasksAsync(new SessionInfo("sessionId1"),
                                                    taskNodes);

    // Assert
    mockBlobService.Verify(m => m.CreateBlobsAsync(It.IsAny<SessionInfo>(),
                                                   It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                                                   It.IsAny<CancellationToken>()),
                           Times.Once);

    ClassicAssert.AreEqual("dependencyBlobId",
                           taskNodes.First()
                                    .DataDependencies.First()
                                    .BlobId);
    ClassicAssert.AreEqual("dependencyBlobId",
                           result.First()
                                 .DataDependencies.First());
  }

  [Test]
  public async Task SubmitTasksAsync_EmptyDataDependencies_DoesNotCreateBlobs()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };
    var mockCallInvoker = new Mock<CallInvoker>();
    var taskResponse = new SubmitTasksResponse
                       {
                         TaskInfos =
                         {
                           new SubmitTasksResponse.Types.TaskInfo
                           {
                             TaskId    = "taskId1",
                             PayloadId = "payloadId1",
                             ExpectedOutputIds =
                             {
                               "outputId1",
                             },
                           },
                         },
                       };

    // Configure SubmitTask call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<SubmitTasksRequest, SubmitTasksResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<SubmitTasksRequest>()))
                   .Returns(new AsyncUnaryCall<SubmitTasksResponse>(Task.FromResult(taskResponse),
                                                                    Task.FromResult(new Metadata()),
                                                                    () => Status.DefaultSuccess,
                                                                    () => new Metadata(),
                                                                    () =>
                                                                    {
                                                                    }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool      = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);
    var mockBlobService = new Mock<IBlobService>();

    var expectedBlobs = new List<BlobInfo>
                        {
                          new BlobInfo
                          {
                            BlobName  = "dependencyBlob",
                            BlobId    = "dependencyBlobId",
                            SessionId = "sessionId1",
                          },
                        };

    var taskService = TasksServiceFactory.CreateTaskService(objectPool,
                                                            mockBlobService.Object,
                                                            NullLoggerFactory.Instance);

    var dataDependenciesContent = ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

    var taskNodes = new List<TaskNode>
                    {
                      new TaskNode
                      {
                        Payload = new BlobInfo
                                  {
                                    BlobName  = "payloadId",
                                    BlobId    = "blobId",
                                    SessionId = "sessionId1",
                                  },
                        ExpectedOutputs         = expectedBlobs,
                        DataDependenciesContent = dataDependenciesContent,
                      },
                    };

    // Act
    await taskService.SubmitTasksAsync(new SessionInfo("sessionId1"),
                                       taskNodes);

    // Assert
    mockBlobService.Verify(m => m.CreateBlobsAsync(It.IsAny<SessionInfo>(),
                                                   It.IsAny<IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>>(),
                                                   It.IsAny<CancellationToken>()),
                           Times.Never);
    Assert.That(taskNodes.First()
                         .DataDependencies,
                Is.Empty);
  }
}
