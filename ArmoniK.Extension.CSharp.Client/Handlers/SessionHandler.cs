using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

/// <summary>
///     Handles session management operations for an ArmoniK client, providing methods to control the lifecycle of a
///     session.
/// </summary>
public class SessionHandler
{
    private readonly ArmoniKClient ArmoniKClient;
    private readonly SessionInfo SessionInfo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionHandler" /> class.
    /// </summary>
    /// <param name="session">The session information for managing session-related operations.</param>
    /// <param name="armoniKClient">The ArmoniK client used to perform operations on the session.</param>
    public SessionHandler(SessionInfo session, ArmoniKClient armoniKClient)
    {
        ArmoniKClient = armoniKClient;
        SessionInfo = session;
    }

    /// <summary>
    ///     Cancels the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task CancelSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CancelSessionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Closes the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task CloseSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.CloseSessionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Pauses the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task PauseSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PauseSessionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Stops submissions to the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task StopSubmissionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.StopSubmissionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Resumes the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task ResumeSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.ResumeSessionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Purges the session asynchronously, removing all data associated with it.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task PurgeSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.PurgeSessionAsync(SessionInfo, cancellationToken);
    }

    /// <summary>
    ///     Deletes the session asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
    public async Task DeleteSessionAsync(CancellationToken cancellationToken)
    {
        var sessionService = await ArmoniKClient.GetSessionService();
        await sessionService.DeleteSessionAsync(SessionInfo, cancellationToken);
    }
}