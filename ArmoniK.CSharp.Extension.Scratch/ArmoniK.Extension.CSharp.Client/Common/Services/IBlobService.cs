using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    Task<BlobInfo> CreateBlobAsync(Session session, CancellationToken cancellationToken = default);
    Task<BlobInfo> CreateBlobAsync(string name, Session session, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string name, ReadOnlyMemory<byte> content, Session session,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names, Session session,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(int quantity, Session session,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs, Session session,
        CancellationToken cancellationToken = default);
    Task<Blob> DownloadBlob(BlobInfo blobInfo, Session session, CancellationToken cancellationToken = default);
    Task UploadBlob(BlobInfo blobInfo, ReadOnlyMemory<byte> content, Session session, CancellationToken cancellationToken = default);
}