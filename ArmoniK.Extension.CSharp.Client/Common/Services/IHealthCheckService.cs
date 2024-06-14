using System.Collections.Generic;
using System.Threading;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Health;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IHealthCheckService
{
    IAsyncEnumerable<Health> GetHealth(CancellationToken cancellationToken);
}