using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IEventsService
{
  Task WaitForBlobsAsync(SessionInfo         session,
                         ICollection<string> blobIds,
                         CancellationToken   cancellationToken = default);

  Task WaitForBlobsAsync(SessionInfo           session,
                         ICollection<BlobInfo> blobInfos,
                         CancellationToken     cancellationToken = default);
}
