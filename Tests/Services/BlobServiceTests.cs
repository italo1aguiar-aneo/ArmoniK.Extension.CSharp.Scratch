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

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace Tests.Services;

public class BlobServiceTests
{
  private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

  [Test]
  public async Task CreateBlob_ReturnsNewBlobInfo()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    var responseAsync = Task.FromResult(new CreateResultsMetaDataResponse
                                        {
                                          Results =
                                          {
                                            new ResultRaw
                                            {
                                              CompletedAt = DateTime.Now.ToUniversalTime()
                                                                    .ToTimestamp(),
                                              Status    = ResultStatus.Created,
                                              Name      = "blobName",
                                              ResultId  = "blodId",
                                              SessionId = "sessionId",
                                            },
                                          },
                                        });

    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsMetaDataRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(responseAsync,
                                                                              Task.FromResult(new Metadata()),
                                                                              () => Status.DefaultSuccess,
                                                                              () => new Metadata(),
                                                                              () =>
                                                                              {
                                                                              }));


    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var blobService = BlobServiceFactory.CreateBlobService(objectPool,
                                                           NullLoggerFactory.Instance);
    // Act
    var result = await blobService.CreateBlobMetadataAsync(new SessionInfo("sessionId"));

    // Assert
    ClassicAssert.AreEqual("blobName",
                           result.BlobName);
  }

  [Test]
  public async Task CreateBlob_WithName_ReturnsNewBlobInfo()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    var name = "blobName";

    var responseAsync = Task.FromResult(new CreateResultsMetaDataResponse
                                        {
                                          Results =
                                          {
                                            new ResultRaw
                                            {
                                              CompletedAt = DateTime.Now.ToUniversalTime()
                                                                    .ToTimestamp(),
                                              Status    = ResultStatus.Created,
                                              Name      = name,
                                              ResultId  = "blodId",
                                              SessionId = "sessionId",
                                            },
                                          },
                                        });

    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsMetaDataRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(responseAsync,
                                                                              Task.FromResult(new Metadata()),
                                                                              () => Status.DefaultSuccess,
                                                                              () => new Metadata(),
                                                                              () =>
                                                                              {
                                                                              }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var blobService = BlobServiceFactory.CreateBlobService(objectPool,
                                                           NullLoggerFactory.Instance);

    // Act
    var result = await blobService.CreateBlobMetadataAsync(new SessionInfo("sessionId"),
                                                           name);

    // Assert
    ClassicAssert.AreEqual("sessionId",
                           result.SessionId);
    ClassicAssert.AreEqual(name,
                           result.BlobName);
  }

  [Test]
  public async Task CreateBlobAsync_WithIAsyncEnumerableContent_CreatesBlobAndUploadsContent()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    // Setup blob content and name
    var contents = AsyncEnumerable.Range(1,
                                         100)
                                  .Select(x => new ReadOnlyMemory<byte>(new[]
                                                                        {
                                                                          (byte)x,
                                                                        }));
    var name = "blobName";

    var serviceConfigurationResponse = Task.FromResult(new ResultsServiceConfigurationResponse
                                                       {
                                                         DataChunkMaxSize = 500,
                                                       });

    //Configure ResultsServiceConfiguration call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<Empty>()))
                   .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(serviceConfigurationResponse,
                                                                                    Task.FromResult(new Metadata()),
                                                                                    () => Status.DefaultSuccess,
                                                                                    () => new Metadata(),
                                                                                    () =>
                                                                                    {
                                                                                    }));

    // Setup CreateMetadata method with mock response
    var createResponse = Task.FromResult(new CreateResultsResponse
                                         {
                                           Results =
                                           {
                                             new ResultRaw
                                             {
                                               CompletedAt = DateTime.Now.ToUniversalTime()
                                                                     .ToTimestamp(),
                                               Status    = ResultStatus.Created,
                                               Name      = name,
                                               ResultId  = "blodId",
                                               SessionId = "sessionId",
                                             },
                                           },
                                         });

    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsRequest, CreateResultsResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsResponse>(createResponse,
                                                                      Task.FromResult(new Metadata()),
                                                                      () => Status.DefaultSuccess,
                                                                      () => new Metadata(),
                                                                      () =>
                                                                      {
                                                                      }));

    //Setup MockStream Object and MockUploadData

    var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

    var responseTask = Task.FromResult(new UploadResultDataResponse
                                       {
                                         Result = new ResultRaw
                                                  {
                                                    Name     = "anyResult",
                                                    ResultId = "anyResultId",
                                                  },
                                       });

    mockCallInvoker.Setup(invoker => invoker.AsyncClientStreamingCall(It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<CallOptions>()))
                   .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(mockStream.Object,
                                                                                                            responseTask,
                                                                                                            Task.FromResult(new Metadata()),
                                                                                                            () => Status.DefaultSuccess,
                                                                                                            () => new Metadata(),
                                                                                                            () =>
                                                                                                            {
                                                                                                            }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var blobService = BlobServiceFactory.CreateBlobService(objectPool,
                                                           NullLoggerFactory.Instance);

    // Act
    var result = await blobService.CreateBlobFromChunksAsync(new SessionInfo("sessionId"),
                                                             name,
                                                             contents);

    // Assert
    ClassicAssert.AreEqual("sessionId",
                           result.SessionId);
    ClassicAssert.AreEqual(name,
                           result.BlobName);
  }

  [Test]
  public async Task CreateBlobAsync_WithContent_CreatesBlobAndUploadsContent()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    var name = "blobName";
    var contents = new ReadOnlyMemory<byte>(Enumerable.Range(1,
                                                             20)
                                                      .Select(x => (byte)x)
                                                      .ToArray());

    var serviceConfigurationResponse = Task.FromResult(new ResultsServiceConfigurationResponse
                                                       {
                                                         DataChunkMaxSize = 500,
                                                       });

    //Configure ResultsServiceConfiguration call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<Empty>()))
                   .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(serviceConfigurationResponse,
                                                                                    Task.FromResult(new Metadata()),
                                                                                    () => Status.DefaultSuccess,
                                                                                    () => new Metadata(),
                                                                                    () =>
                                                                                    {
                                                                                    }));

    var metadataCreationResponse = Task.FromResult(new CreateResultsMetaDataResponse
                                                   {
                                                     Results =
                                                     {
                                                       new ResultRaw
                                                       {
                                                         CompletedAt = DateTime.Now.ToUniversalTime()
                                                                               .ToTimestamp(),
                                                         Status    = ResultStatus.Created,
                                                         Name      = name,
                                                         ResultId  = "blodId",
                                                         SessionId = "sessionId",
                                                       },
                                                     },
                                                   });

    //Configure CreateResultsMetaData call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsMetaDataRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(metadataCreationResponse,
                                                                              Task.FromResult(new Metadata()),
                                                                              () => Status.DefaultSuccess,
                                                                              () => new Metadata(),
                                                                              () =>
                                                                              {
                                                                              }));

    var createResultResponse = Task.FromResult(new CreateResultsResponse
                                               {
                                                 Results =
                                                 {
                                                   new ResultRaw
                                                   {
                                                     CompletedAt = DateTime.Now.ToUniversalTime()
                                                                           .ToTimestamp(),
                                                     Status    = ResultStatus.Created,
                                                     Name      = name,
                                                     ResultId  = "blodId",
                                                     SessionId = "sessionId",
                                                   },
                                                 },
                                               });

    //Configure CreateResults call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsRequest, CreateResultsResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsResponse>(createResultResponse,
                                                                      Task.FromResult(new Metadata()),
                                                                      () => Status.DefaultSuccess,
                                                                      () => new Metadata(),
                                                                      () =>
                                                                      {
                                                                      }));


    var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

    var responseTask = Task.FromResult(new UploadResultDataResponse
                                       {
                                         Result = new ResultRaw
                                                  {
                                                    Name     = "anyResult",
                                                    ResultId = "anyResultId",
                                                  },
                                       });

    //Configure UploadResultData call
    mockCallInvoker.Setup(invoker => invoker.AsyncClientStreamingCall(It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<CallOptions>()))
                   .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(mockStream.Object,
                                                                                                            responseTask,
                                                                                                            Task.FromResult(new Metadata()),
                                                                                                            () => Status.DefaultSuccess,
                                                                                                            () => new Metadata(),
                                                                                                            () =>
                                                                                                            {
                                                                                                            }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var blobService = BlobServiceFactory.CreateBlobService(objectPool,
                                                           NullLoggerFactory.Instance);

    // Act
    var result = await blobService.CreateBlobAsync(new SessionInfo("sessionId"),
                                                   name,
                                                   contents);

    // Assert
    ClassicAssert.AreEqual("sessionId",
                           result.SessionId);
    ClassicAssert.AreEqual(name,
                           result.BlobName);
  }

  [Test]
  public async Task CreateBlobAsync_WithBigContent_CreatesBlobAndUploadsContent()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    var name = "blobName";
    var contents = new ReadOnlyMemory<byte>(Enumerable.Range(1,
                                                             100)
                                                      .Select(x => (byte)x)
                                                      .ToArray());

    var resultsServiceConfiguration = Task.FromResult(new ResultsServiceConfigurationResponse
                                                      {
                                                        DataChunkMaxSize = 20,
                                                      });

    //Configure ResultsServiceConfiguration call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<Empty, ResultsServiceConfigurationResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<Empty>()))
                   .Returns(new AsyncUnaryCall<ResultsServiceConfigurationResponse>(resultsServiceConfiguration,
                                                                                    Task.FromResult(new Metadata()),
                                                                                    () => Status.DefaultSuccess,
                                                                                    () => new Metadata(),
                                                                                    () =>
                                                                                    {
                                                                                    }));

    var createMetadataResponse = Task.FromResult(new CreateResultsMetaDataResponse
                                                 {
                                                   Results =
                                                   {
                                                     new ResultRaw
                                                     {
                                                       CompletedAt = DateTime.Now.ToUniversalTime()
                                                                             .ToTimestamp(),
                                                       Status    = ResultStatus.Created,
                                                       Name      = name,
                                                       ResultId  = "blodId",
                                                       SessionId = "sessionId",
                                                     },
                                                   },
                                                 });

    //Configure CreateResultsMetaData call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsMetaDataRequest, CreateResultsMetaDataResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsMetaDataRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsMetaDataResponse>(createMetadataResponse,
                                                                              Task.FromResult(new Metadata()),
                                                                              () => Status.DefaultSuccess,
                                                                              () => new Metadata(),
                                                                              () =>
                                                                              {
                                                                              }));

    var createResultsResponse = Task.FromResult(new CreateResultsResponse
                                                {
                                                  Results =
                                                  {
                                                    new ResultRaw
                                                    {
                                                      CompletedAt = DateTime.Now.ToUniversalTime()
                                                                            .ToTimestamp(),
                                                      Status    = ResultStatus.Created,
                                                      Name      = name,
                                                      ResultId  = "blodId",
                                                      SessionId = "sessionId",
                                                    },
                                                  },
                                                });

    //Configure CreateResults call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateResultsRequest, CreateResultsResponse>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateResultsRequest>()))
                   .Returns(new AsyncUnaryCall<CreateResultsResponse>(createResultsResponse,
                                                                      Task.FromResult(new Metadata()),
                                                                      () => Status.DefaultSuccess,
                                                                      () => new Metadata(),
                                                                      () =>
                                                                      {
                                                                      }));

    var mockStream = new Mock<IClientStreamWriter<UploadResultDataRequest>>();

    var responseTask = Task.FromResult(new UploadResultDataResponse
                                       {
                                         Result = new ResultRaw
                                                  {
                                                    Name     = "anyResult",
                                                    ResultId = "anyResultId",
                                                  },
                                       });

    //Configure UploadResultData call
    mockCallInvoker.Setup(invoker => invoker.AsyncClientStreamingCall(It.IsAny<Method<UploadResultDataRequest, UploadResultDataResponse>>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<CallOptions>()))
                   .Returns(new AsyncClientStreamingCall<UploadResultDataRequest, UploadResultDataResponse>(mockStream.Object,
                                                                                                            responseTask,
                                                                                                            Task.FromResult(new Metadata()),
                                                                                                            () => Status.DefaultSuccess,
                                                                                                            () => new Metadata(),
                                                                                                            () =>
                                                                                                            {
                                                                                                            }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var blobService = BlobServiceFactory.CreateBlobService(objectPool,
                                                           NullLoggerFactory.Instance);

    // Act
    var result = await blobService.CreateBlobAsync(new SessionInfo("sessionId"),
                                                   name,
                                                   contents);

    // Assert
    ClassicAssert.AreEqual("sessionId",
                           result.SessionId);
    ClassicAssert.AreEqual(name,
                           result.BlobName);
  }
}
