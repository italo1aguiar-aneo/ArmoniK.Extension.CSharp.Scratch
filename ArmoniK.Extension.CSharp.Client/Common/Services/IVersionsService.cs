using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Versions;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

/// <summary>
///     Defines a service for retrieving version information.
/// </summary>
public interface IVersionsService
{
    /// <summary>
    ///     Asynchronously retrieves version information.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the version information.</returns>
    Task<VersionsInfo> GetVersion(CancellationToken cancellationToken);
}