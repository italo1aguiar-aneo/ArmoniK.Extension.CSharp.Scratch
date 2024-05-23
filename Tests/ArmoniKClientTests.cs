using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Tests;

public class ArmoniKClientTests
{
    private static IConfiguration _configuration;
    private readonly ArmoniKClient _client;
    private readonly List<string> _defaultPartitionsIds;
    private readonly Properties _defaultProperties;
    private readonly TaskOptions _defaultTaskOptions;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public ArmoniKClientTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.tests.json", false)
            .AddEnvironmentVariables().Build();

        _defaultPartitionsIds = new List<string> { "subtasking" };

        _defaultTaskOptions = new TaskOptions
        {
            MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
            MaxRetries = 2,
            Priority = 1,
            PartitionId = _defaultPartitionsIds[0]
        };

        _defaultProperties = new Properties(_configuration, _defaultTaskOptions, _defaultPartitionsIds);

        _loggerFactoryMock = new Mock<ILoggerFactory>();

        _client = new ArmoniKClient(_defaultProperties, _loggerFactoryMock.Object);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_IfPropertiesIsNull()
    {
        // Act 
        var exception = Assert.Throws<ArgumentNullException>(() => new ArmoniKClient(null, _loggerFactoryMock.Object));

        // Assert
        ClassicAssert.AreEqual("properties", exception.ParamName);
    }

    [Test]
    public void Constructor_ThrowsArgumentNullException_IfLoggerFactoryIsNull()
    {
        // Act  
        var exception = Assert.Throws<ArgumentNullException>(() => new ArmoniKClient(_defaultProperties, null));
        // Assert
        ClassicAssert.AreEqual("loggerFactory", exception.ParamName);
    }

    [Test]
    public async Task GetBlobService_ShouldReturnInstance()
    {
        // Arrange
        var session = new Session
        {
            Id = Guid.NewGuid().ToString()
        };

        // Act
        var blobService = await _client.GetBlobService(session);

        // Assert
        Assert.That(blobService, Is.InstanceOf<IBlobService>(),
            "The returned object should be an instance of IBlobService or derive from it.");
    }

    [Test]
    public async Task GetBlobService_CachesInstance()
    {
        // Arrange
        var session1 = new Session
        {
            Id = Guid.NewGuid().ToString()
        };
        var session2 = new Session
        {
            Id = Guid.NewGuid().ToString()
        };

        // Act
        var blobService1 = await _client.GetBlobService(session1);
        var blobService2 = await _client.GetBlobService(session1);
        var blobService3 = await _client.GetBlobService(session2);


        // Assert
        ClassicAssert.AreEqual(blobService1, blobService2);
        ClassicAssert.AreNotEqual(blobService1, blobService3);
    }

    [Test]
    public async Task GetSessionService_ShouldReturnInstance()
    {
        // Act
        var sessionService = await _client.GetSessionService();

        // Assert
        Assert.That(sessionService, Is.InstanceOf<ISessionService>(),
            "The returned object should be an instance of ISessionService or derive from it.");
    }

    [Test]
    public async Task GetTasksService_ShouldReturnInstance()
    {
        // Arrange
        var session = new Session { Id = Guid.NewGuid().ToString() };

        // Act
        var taskService = await _client.GetTasksService(session);

        // Assert
        Assert.That(taskService, Is.InstanceOf<ITasksService>(),
            "The returned object should be an instance of ITasksService or derive from it.");
    }

    [Test]
    public async Task GetTasksService_CachesInstance()
    {
        // Arrange
        var session1 = new Session { Id = Guid.NewGuid().ToString() };
        var session2 = new Session { Id = Guid.NewGuid().ToString() };

        // Act
        var taskService1 = await _client.GetTasksService(session1);
        var taskService2 = await _client.GetTasksService(session1);
        var taskService3 = await _client.GetTasksService(session2);

        // Assert
        ClassicAssert.AreEqual(taskService1, taskService2);
        ClassicAssert.AreNotEqual(taskService1, taskService3);
    }

    [Test]
    public async Task GetEventsService_ShouldReturnInstance()
    {
        // Arrange
        var session = new Session { Id = Guid.NewGuid().ToString() };

        // Act
        var eventsService = await _client.GetEventsService(session);

        // Assert
        Assert.That(eventsService, Is.InstanceOf<IEventsService>(),
            "The returned object should be an instance of IEventsService or derive from it.");
    }

    [Test]
    public async Task GetEventsService_CachesInstance()
    {
        // Arrange
        var session1 = new Session { Id = Guid.NewGuid().ToString() };
        var session2 = new Session { Id = Guid.NewGuid().ToString() };

        // Act
        var eventsService1 = await _client.GetEventsService(session1);
        var eventsService2 = await _client.GetEventsService(session1);
        var eventsService3 = await _client.GetEventsService(session2);

        // Assert
        ClassicAssert.AreEqual(eventsService1, eventsService2);
        ClassicAssert.AreNotEqual(eventsService1, eventsService3);
    }
}