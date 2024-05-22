using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class SessionService : ISessionService
{
    private readonly ObjectPool<ChannelBase> _channel;
    private readonly ILogger<SessionService> _logger;

    private readonly Properties _properties;

    public SessionService(ObjectPool<ChannelBase> channel, Properties properties, ILoggerFactory loggerFactory)
    {
        _properties = properties;
        _logger = loggerFactory.CreateLogger<SessionService>();
        _channel = channel;
    }

    public async Task<Session> CreateSession()
    {
        await using var channel = await _channel.GetAsync();
        var sessionClient = new Sessions.SessionsClient(channel);
        var createSessionReply = await sessionClient.CreateSessionAsync(new CreateSessionRequest
        {
            DefaultTaskOption = _properties.TaskOptions,
            PartitionIds =
            {
                _properties.PartitionIds
            }
        });

        return new Session
        {
            Id = createSessionReply.SessionId
        };
    }
}