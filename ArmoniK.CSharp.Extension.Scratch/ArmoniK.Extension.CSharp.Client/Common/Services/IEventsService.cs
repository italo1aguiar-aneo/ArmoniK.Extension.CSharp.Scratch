using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IEventsService
{
    Task WaitForBlobsAsync(ICollection<string> blobIds,
        CancellationToken cancellationToken = default);

    Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos,
        CancellationToken cancellationToken = default);
}