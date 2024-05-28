using System.Threading;
using System.Threading.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ISessionService
{
    Task<string> CreateSessionAsync(CancellationToken cancellationToken = default);
    Task CancelSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task PauseSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task StopSubmissionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task ResumeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task PurgeSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}