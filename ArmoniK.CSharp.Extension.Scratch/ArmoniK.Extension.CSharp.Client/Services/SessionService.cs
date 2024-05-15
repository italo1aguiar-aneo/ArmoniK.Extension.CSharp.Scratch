using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Api.gRPC.V1.Submitter;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Abstracts;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Microsoft.Extensions.Logging;
using CreateSessionRequest = ArmoniK.Api.gRPC.V1.Sessions.CreateSessionRequest;

namespace ArmoniK.Extension.CSharp.Client.Services
{
    public class SessionService : BaseArmoniKClientService<SessionService>, ISessionService
    {


        public Session Session;

        public SessionService(Properties properties, ILoggerFactory loggerFactory,
            TaskOptions taskOptions = null) : base(
            properties, loggerFactory, taskOptions ?? ClientServiceConnector.InitializeDefaultTaskOptions())
        {
            _sessionClient = new Sessions.SessionsClient(ChannelPool.Get());
        }

        private Sessions.SessionsClient _sessionClient;
        private readonly object _lock = new();

        // Property to access the SessionClient
        private async Task<Sessions.SessionsClient> GetSessionClientAsync()
        {
            if (_sessionClient == null)
            {
                await InitializeSessionClientAsync();
            }

            return _sessionClient;
        }

        // Method to initialize the SessionClient
        private async Task InitializeSessionClientAsync()
        {
            if (_sessionClient == null)
            {
                var client = new Sessions.SessionsClient(await ChannelPool.GetAsync());
                lock (_lock)
                {
                    _sessionClient ??= client;
                }
            }
        }

        public async Task<Session> CreateSession(IEnumerable<string> partitionIds)
        {
            var sessionClient = await GetSessionClientAsync();
            var createSessionReply = await sessionClient.CreateSessionAsync(new CreateSessionRequest
            {
                DefaultTaskOption = TaskOptions,
                PartitionIds =
                {
                    partitionIds
                }
            });

            Session = new Session()
            {
                Id = createSessionReply.SessionId
            };

            return Session;
        }
    }
}