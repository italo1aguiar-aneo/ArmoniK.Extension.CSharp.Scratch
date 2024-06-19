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
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

namespace Tests.Services;

public class EventsServiceTests
{
  private readonly List<string> defaultPartitionsIds_;

  public EventsServiceTests()
    => defaultPartitionsIds_ = new List<string>
                               {
                                 "subtasking",
                               };

  [Test]
  public async Task CreateSession_ReturnsNewSessionWithId()
  {
    // Arrange
    var mockChannelBase = new Mock<ChannelBase>("localhost")
                          {
                            CallBase = true,
                          };

    // Setup the abstract CreateCallInvoker method to return a mock CallInvoker
    var mockCallInvoker = new Mock<CallInvoker>();

    var responses = new Queue<EventSubscriptionResponse>(new[]
                                                         {
                                                           new EventSubscriptionResponse
                                                           {
                                                             SessionId = "1234",
                                                             NewResult = new EventSubscriptionResponse.Types.NewResult
                                                                         {
                                                                           ResultId = "1234",
                                                                           Status   = ResultStatus.Completed,
                                                                         },
                                                           },
                                                         });

    // Setup the mock stream reader
    var streamReaderMock = new Mock<IAsyncStreamReader<EventSubscriptionResponse>>();
    streamReaderMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                    .Returns(() => Task.FromResult(responses.Count > 0))
                    .Returns(() => Task.FromResult(false));

    streamReaderMock.SetupGet(x => x.Current)
                    .Returns(() => responses.Dequeue());

    mockCallInvoker.Setup(invoker => invoker.AsyncServerStreamingCall(It.IsAny<Method<EventSubscriptionRequest, EventSubscriptionResponse>>(),
                                                                      It.IsAny<string>(),
                                                                      It.IsAny<CallOptions>(),
                                                                      It.IsAny<EventSubscriptionRequest>()))
                   .Returns(new AsyncServerStreamingCall<EventSubscriptionResponse>(streamReaderMock.Object,
                                                                                    Task.FromResult(new Metadata()),
                                                                                    () => Status.DefaultSuccess,
                                                                                    () => new Metadata(),
                                                                                    () =>
                                                                                    {
                                                                                    }));

    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var eventsService = EventsServiceFactory.CreateEventsService(objectPool,
                                                                 NullLoggerFactory.Instance);
    // Act

    var blobId = new List<string>
                 {
                   "1234",
                 };

    Assert.DoesNotThrowAsync(async () => await eventsService.WaitForBlobsAsync(new SessionInfo("sessionId"),
                                                                               blobId));
  }
}
