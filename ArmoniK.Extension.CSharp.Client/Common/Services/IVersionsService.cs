using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Versions;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IVersionsService
{
    Task<VersionsInfo> GetVersion(CancellationToken cancellationToken);
}