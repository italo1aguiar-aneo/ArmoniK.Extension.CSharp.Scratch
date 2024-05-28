using System.Threading;
using System.Threading.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class SessionHandler
{
    public readonly ArmoniKClient ArmoniKClient;
    public readonly string SessionId;

    public SessionHandler(string sessionId, ArmoniKClient armoniKClient)
    {
        SessionId = sessionId;
        ArmoniKClient = armoniKClient;
    }

    public async Task CancelSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CancelSessionAsync(SessionId, cancellationToken);
    }

    public async Task CloseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CloseSessionAsync(SessionId, cancellationToken);
    }

    public async Task PauseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PauseSessionAsync(SessionId, cancellationToken);
    }

    public async Task StopSubmission(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.StopSubmissionAsync(SessionId, cancellationToken);
    }

    public async Task ResumeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.ResumeSessionAsync(SessionId, cancellationToken);
    }

    public async Task PurgeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PurgeSessionAsync(SessionId, cancellationToken);
    }

    public async Task DeleteSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.DeleteSessionAsync(SessionId, cancellationToken);
    }
}