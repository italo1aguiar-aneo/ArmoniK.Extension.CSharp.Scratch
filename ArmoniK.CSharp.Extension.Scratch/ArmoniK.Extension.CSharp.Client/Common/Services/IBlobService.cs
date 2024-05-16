using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    Task<BlobInfo> CreateBlobAsync(Blob blob, Session session, CancellationToken cancellationToken = default);
    Task<BlobInfo> CreateBlobAsync(BlobInfo blobInfo, Session session, CancellationToken cancellationToken = default);

    Task<ICollection<BlobInfo>> CreateBlobsAsync(ICollection<BlobInfo> blobsInfos, Session session,
        CancellationToken cancellationToken = default);

    Task<ICollection<BlobInfo>> CreateBlobsAsync(ICollection<Blob> blobs, Session session,
        CancellationToken cancellationToken = default);

    Task<Blob> DownloadBlob(BlobInfo blobInfo, Session session, CancellationToken cancellationToken = default);
}