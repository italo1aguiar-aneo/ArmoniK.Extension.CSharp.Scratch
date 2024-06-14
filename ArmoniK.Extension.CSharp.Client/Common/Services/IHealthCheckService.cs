using System.Collections.Generic;
using System.Threading;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Health;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

/// <summary>
///     Defines a service for performing health checks on the system components.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    ///     Asynchronously retrieves the health status of the system components.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of health statuses.</returns>
    IAsyncEnumerable<Health> GetHealth(CancellationToken cancellationToken);
}