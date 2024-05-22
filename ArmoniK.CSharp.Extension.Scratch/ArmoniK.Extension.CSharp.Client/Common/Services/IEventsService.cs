using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IEventsService
{
    Task WaitForBlobsAsync(ICollection<string> blobIds, Session session = null,
        CancellationToken cancellationToken = default);

    Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos, Session session = null,
        CancellationToken cancellationToken = default);
}