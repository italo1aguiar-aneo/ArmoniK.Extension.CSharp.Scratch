using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

/// <summary>
///     Defines a service for managing sessions, including creating, canceling, closing, and modifying sessions.
/// </summary>
public interface ISessionService
{
    /// <summary>
    ///     Asynchronously creates a new session.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the created session information.</returns>
    Task<SessionInfo> CreateSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously cancels a specified session.
    /// </summary>
    /// <param name="session">The session information to cancel.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CancelSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously closes a specified session.
    /// </summary>
    /// <param name="session">The session information to close.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously pauses a specified session.
    /// </summary>
    /// <param name="session">The session information to pause.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PauseSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously stops submissions for a specified session.
    /// </summary>
    /// <param name="session">The session information to stop submissions for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopSubmissionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously resumes a specified session.
    /// </summary>
    /// <param name="session">The session information to resume.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResumeSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously purges a specified session, removing all data associated with it.
    /// </summary>
    /// <param name="session">The session information to purge.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PurgeSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously deletes a specified session.
    /// </summary>
    /// <param name="session">The session information to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
}