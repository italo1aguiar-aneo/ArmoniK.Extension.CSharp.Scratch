using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Utils;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IEventsService
{
    Task WaitForBlobsAsync(SessionInfo session, ICollection<BlobInfo> blobInfos,
        CancellationToken cancellationToken = default);
}

public static class IEventsServiceExt
{
    public static Task WaitForBlobsAsync(this IEventsService eventsService, SessionInfo session,
        ICollection<string> blobInfos,
        CancellationToken cancellationToken = default)
    {
        return eventsService.WaitForBlobsAsync(session,
            blobInfos.Select(info => new BlobInfo
            {
                BlobId = info,
                BlobName = "",
                SessionId = session.SessionId
            }).AsICollection(), cancellationToken);
    }
}