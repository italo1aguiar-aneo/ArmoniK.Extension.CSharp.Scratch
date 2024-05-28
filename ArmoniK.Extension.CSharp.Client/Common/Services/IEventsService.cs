using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IEventsService
{
    Task WaitForBlobsAsync(string sessionId, ICollection<string> blobIds,
        CancellationToken cancellationToken = default);

    Task WaitForBlobsAsync(string sessionId, ICollection<BlobInfo> blobInfos,
        CancellationToken cancellationToken = default);
}