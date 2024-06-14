using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class SessionHandler
{
    private readonly ArmoniKClient ArmoniKClient;

    private readonly SessionInfo SessionInfo;

    public SessionHandler(SessionInfo session, ArmoniKClient armoniKClient)
    {
        ArmoniKClient = armoniKClient;
        SessionInfo = session;
    }

    public async Task CancelSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CancelSessionAsync(SessionInfo, cancellationToken);
    }

    public async Task CloseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CloseSessionAsync(SessionInfo, cancellationToken);
    }

    public async Task PauseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PauseSessionAsync(SessionInfo, cancellationToken);
    }

    public async Task StopSubmission(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.StopSubmissionAsync(SessionInfo, cancellationToken);
    }

    public async Task ResumeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.ResumeSessionAsync(SessionInfo, cancellationToken);
    }

    public async Task PurgeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PurgeSessionAsync(SessionInfo, cancellationToken);
    }

    public async Task DeleteSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.DeleteSessionAsync(SessionInfo, cancellationToken);
    }
}