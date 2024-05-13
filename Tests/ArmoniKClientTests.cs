using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
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
    private readonly TaskConfiguration _defaultTaskOptions;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public ArmoniKClientTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.tests.json", false)
            .AddEnvironmentVariables().Build();

        _defaultPartitionsIds = new List<string> { "subtasking" };

        _defaultTaskOptions = new TaskConfiguration(
            2,
            1,
            "subtasking",
            TimeSpan.FromHours(1)
        );

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
        var blobService = await _client.GetBlobService();

        // Assert
        Assert.That(blobService, Is.InstanceOf<IBlobService>(),
            "The returned object should be an instance of IBlobService or derive from it.");
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
        var taskService = await _client.GetTasksService();

        // Assert
        Assert.That(taskService, Is.InstanceOf<ITasksService>(),
            "The returned object should be an instance of ITasksService or derive from it.");
    }

    [Test]
    public async Task GetEventsService_ShouldReturnInstance()
    {
        // Arrange
        var session = new Session { Id = Guid.NewGuid().ToString() };

        // Act
        var eventsService = await _client.GetEventsService();

        // Assert
        Assert.That(eventsService, Is.InstanceOf<IEventsService>(),
            "The returned object should be an instance of IEventsService or derive from it.");
    }
}