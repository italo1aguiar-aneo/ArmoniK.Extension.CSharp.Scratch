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
  private readonly List<string>                  _defaultPartitionsIds;
  private readonly Properties                    _defaultProperties;
  private readonly Mock<ObjectPool<ChannelBase>> _mockChannelPool;

  public SessionServiceTests()
  {
    var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                  .AddJsonFile("appsettings.tests.json",
                                                               false)
                                                  .AddEnvironmentVariables()
                                                  .Build();

    _defaultPartitionsIds = new List<string>
                            {
                              "subtasking",
                            };

    var defaultTaskOptions = new TaskConfiguration(2,
                                                   1,
                                                   _defaultPartitionsIds[0],
                                                   TimeSpan.FromHours(1));

    _defaultProperties = new Properties(configuration,
                                        defaultTaskOptions,
                                        _defaultPartitionsIds);
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
                                                                    _defaultProperties,
                                                                    NullLoggerFactory.Instance);
    // Act
    var result = await sessionService.CreateSessionAsync();

    // Assert
    ClassicAssert.AreEqual("12345",
                           result.SessionId);
  }
}
