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

using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests.Services;

public class SessionServiceTests
{
  private readonly Properties defaultProperties_;

  public SessionServiceTests()
  {
    var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                  .AddJsonFile("appsettings.tests.json",
                                                               false)
                                                  .AddEnvironmentVariables()
                                                  .Build();

    List<string> defaultPartitionsIds = new()
                                        {
                                          "subtasking",
                                        };

    var defaultTaskOptions = new TaskConfiguration(2,
                                                   1,
                                                   defaultPartitionsIds[0],
                                                   TimeSpan.FromHours(1));

    defaultProperties_ = new Properties(configuration,
                                        defaultTaskOptions,
                                        defaultPartitionsIds);
  }

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

    // Configure CreateSession call
    mockCallInvoker.Setup(invoker => invoker.AsyncUnaryCall(It.IsAny<Method<CreateSessionRequest, CreateSessionReply>>(),
                                                            It.IsAny<string>(),
                                                            It.IsAny<CallOptions>(),
                                                            It.IsAny<CreateSessionRequest>()))
                   .Returns(new AsyncUnaryCall<CreateSessionReply>(Task.FromResult(new CreateSessionReply
                                                                                   {
                                                                                     SessionId = "12345",
                                                                                   }),
                                                                   Task.FromResult(new Metadata()),
                                                                   () => Status.DefaultSuccess,
                                                                   () => new Metadata(),
                                                                   () =>
                                                                   {
                                                                   }));


    mockChannelBase.Setup(m => m.CreateCallInvoker())
                   .Returns(mockCallInvoker.Object);

    var objectPool = new ObjectPool<ChannelBase>(() => mockChannelBase.Object);

    var sessionService = SessionServiceFactory.CreateSessionService(objectPool,
                                                                    defaultProperties_,
                                                                    NullLoggerFactory.Instance);
    // Act
    var result = await sessionService.CreateSessionAsync();

    // Assert
    ClassicAssert.AreEqual("12345",
                           result.SessionId);
  }
}
