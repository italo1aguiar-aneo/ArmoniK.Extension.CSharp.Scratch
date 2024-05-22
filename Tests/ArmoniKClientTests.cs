using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Services;
using Castle.Components.DictionaryAdapter.Xml;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit;

namespace Tests
{
    public class ArmoniKClientTests
    {
        private static IConfiguration _configuration;
        private readonly Properties _defaultProperties;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly TaskOptions _defaultTaskOptions;
        private readonly List<string> _defaultPartitionsIds;
        private readonly ArmoniKClient _client;
        public ArmoniKClientTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.tests.json", false)
                .AddEnvironmentVariables().Build();

            _defaultPartitionsIds = new List<string>(){ "subtasking" };

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

        [Fact]
        public void Constructor_ThrowsArgumentNullException_IfPropertiesIsNull()
        {
            // Act 
            var exception = Assert.Throws<ArgumentNullException>(() => new ArmoniKClient(null, _loggerFactoryMock.Object));
            
            // Assert
            Assert.Equal("properties", exception.ParamName);
        }
        [Fact]
        public void Constructor_ThrowsArgumentNullException_IfLoggerFactoryIsNull()
        {
            // Act  
            var exception = Assert.Throws<ArgumentNullException>(() => new ArmoniKClient(_defaultProperties, null));
            // Assert
            Assert.Equal("loggerFactory", exception.ParamName);
        }

        [Fact]
        public async Task GetBlobService_ShouldReturnInstance()
        {
            // Arrange
            var session = new Session()
            {
                Id = Guid.NewGuid().ToString()
            }; 

            // Act
            var blobService = await _client.GetBlobService(session);

            // Assert
            Assert.IsAssignableFrom<IBlobService>(blobService);
        }
        [Fact]
        public async Task GetBlobService_CachesInstance()
        {
            // Arrange
            var session1 = new Session()
            {
                Id = Guid.NewGuid().ToString()
            };
            var session2 = new Session()
            {
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var blobService1 = await _client.GetBlobService(session1);
            var blobService2 = await _client.GetBlobService(session1);
            var blobService3 = await _client.GetBlobService(session2);


            // Assert
            Assert.Same(blobService1, blobService2);
            Assert.NotSame(blobService1, blobService3);
        }
        [Fact]
        public async Task GetSessionService_ShouldReturnInstance()
        {
            // Act
            var sessionService = await _client.GetSessionService();

            // Assert
            Assert.IsAssignableFrom<ISessionService>(sessionService);
        }

        [Fact]
        public async Task GetTasksService_ShouldReturnInstance()
        {
            // Arrange
            var session = new Session() { Id = Guid.NewGuid().ToString() };

            // Act
            var taskService = await _client.GetTasksService(session);

            // Assert
            Assert.IsAssignableFrom<ITasksService>(taskService);
        }

        [Fact]
        public async Task GetTasksService_CachesInstance()
        {
            // Arrange
            var session1 = new Session() { Id = Guid.NewGuid().ToString() };
            var session2 = new Session() { Id = Guid.NewGuid().ToString() };

            // Act
            var taskService1 = await _client.GetTasksService(session1);
            var taskService2 = await _client.GetTasksService(session1);
            var taskService3 = await _client.GetTasksService(session2);

            // Assert
            Assert.Same(taskService1, taskService2);
            Assert.NotSame(taskService1, taskService3);
        }
        [Fact]
        public async Task GetEventsService_ShouldReturnInstance()
        {
            // Arrange
            var session = new Session() { Id = Guid.NewGuid().ToString() };

            // Act
            var eventsService = await _client.GetEventsService(session);

            // Assert
            Assert.IsAssignableFrom<IEventsService>(eventsService);
        }

        [Fact]
        public async Task GetEventsService_CachesInstance()
        {
            // Arrange
            var session1 = new Session() { Id = Guid.NewGuid().ToString() };
            var session2 = new Session() { Id = Guid.NewGuid().ToString() };

            // Act
            var eventsService1 = await _client.GetEventsService(session1);
            var eventsService2 = await _client.GetEventsService(session1);
            var eventsService3 = await _client.GetEventsService(session2);

            // Assert
            Assert.Same(eventsService1, eventsService2);
            Assert.NotSame(eventsService1, eventsService3);
        }
    }
}
