using System.Threading;
using System.Threading.Tasks;
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

    public async Task<string> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        var createSessionReply = await sessionClient.CreateSessionAsync(new CreateSessionRequest
        {
            DefaultTaskOption = _properties.TaskOptions.ToTaskOptions(),
            PartitionIds =
            {
                _properties.PartitionIds
            }
        });

        return createSessionReply.SessionId;
    }

    public async Task CancelSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.CancelSessionAsync(new CancelSessionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.CloseSessionAsync(new CloseSessionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task PauseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.PauseSessionAsync(new PauseSessionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task StopSubmissionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.StopSubmissionAsync(new StopSubmissionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task ResumeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.ResumeSessionAsync(new ResumeSessionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task PurgeSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.PurgeSessionAsync(new PurgeSessionRequest
        {
            SessionId = sessionId
        });
    }

    public async Task DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken);
        var sessionClient = new Sessions.SessionsClient(channel);
        await sessionClient.DeleteSessionAsync(new DeleteSessionRequest
        {
            SessionId = sessionId
        });
    }
}