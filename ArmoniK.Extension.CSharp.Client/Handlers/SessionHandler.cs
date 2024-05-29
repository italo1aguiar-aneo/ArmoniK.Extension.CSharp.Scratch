using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using System.Threading;
using System.Threading.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class SessionHandler: SessionInfo
{
    public readonly ArmoniKClient ArmoniKClient;

    public SessionHandler(string sessionId, ArmoniKClient armoniKClient): base(sessionId)
    {
        ArmoniKClient = armoniKClient;
    }

    public async Task CancelSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CancelSessionAsync(this, cancellationToken);
    }

    public async Task CloseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CloseSessionAsync(this, cancellationToken);
    }

    public async Task PauseSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PauseSessionAsync(this, cancellationToken);
    }

    public async Task StopSubmission(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.StopSubmissionAsync(this, cancellationToken);
    }

    public async Task ResumeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.ResumeSessionAsync(this, cancellationToken);
    }

    public async Task PurgeSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PurgeSessionAsync(this, cancellationToken);
    }

    public async Task DeleteSession(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.DeleteSessionAsync(this, cancellationToken);
    }
}