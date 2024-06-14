using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Tests.Helpers;

namespace Tests.Services;

public class SessionServiceTests
{
    private readonly List<string> _defaultPartitionsIds;
    private readonly Properties _defaultProperties;

    public SessionServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.tests.json", false)
            .AddEnvironmentVariables().Build();

        _defaultPartitionsIds = new List<string> { "subtasking" };

        var defaultTaskOptions = new TaskConfiguration(
            2,
            1,
            _defaultPartitionsIds[0],
            TimeSpan.FromHours(1)
        );

        _defaultProperties = new Properties(configuration, defaultTaskOptions, _defaultPartitionsIds);
    }

    [Test]
    public async Task CreateSession_ReturnsNewSessionWithId()
    {
        var mockCallInvoker = new Mock<CallInvoker>();

        var createSessionReply = new CreateSessionReply { SessionId = "12345" };

        mockCallInvoker.SetupAsyncUnaryCallInvokerMock<CreateSessionRequest, CreateSessionReply>(createSessionReply);

        var sessionService = MockHelper.GetSessionServiceMock(mockCallInvoker, _defaultProperties);
        // Act
        var result = await sessionService.CreateSessionAsync();
        // Assert
        ClassicAssert.AreEqual("12345", result.SessionId);
    }
}