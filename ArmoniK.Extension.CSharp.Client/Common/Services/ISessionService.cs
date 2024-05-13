using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ISessionService
{
    Task<SessionInfo> CreateSessionAsync(CancellationToken cancellationToken = default);
    Task CancelSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task CloseSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task PauseSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task StopSubmissionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task ResumeSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task PurgeSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(SessionInfo session, CancellationToken cancellationToken = default);
}