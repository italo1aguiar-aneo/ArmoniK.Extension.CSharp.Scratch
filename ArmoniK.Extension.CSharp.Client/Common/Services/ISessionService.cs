using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ISessionService
{
    Task<Session> CreateSession(CancellationToken cancellationToken = default);
}