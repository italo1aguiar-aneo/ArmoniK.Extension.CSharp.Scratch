using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using CreateSessionRequest = ArmoniK.Api.gRPC.V1.Sessions.CreateSessionRequest;

namespace ArmoniK.Extension.CSharp.Client.Services
{
    public class SessionService : ISessionService
    {
        public SessionService(ChannelBase channel, Properties properties, ILoggerFactory loggerFactory)
        {
            _properties = properties;
            _sessionClient = new Sessions.SessionsClient(channel);
            _logger = loggerFactory.CreateLogger<SessionService>();
        }

        private readonly Properties _properties;
        private readonly Sessions.SessionsClient _sessionClient;
        private readonly ILogger<SessionService> _logger;
        public async Task<Session> CreateSession()
        {
            var createSessionReply = await _sessionClient.CreateSessionAsync(new CreateSessionRequest
            {
                DefaultTaskOption = _properties.TaskOptions,
                PartitionIds =
                {
                    _properties.PartitionIds
                }
            });

            return new Session()
            {
                Id = createSessionReply.SessionId,
            };
        }
    }
}