using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    void SetSession(Session session);

    Task<BlobInfo> CreateBlobAsync(string name, Session session = null,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(Session session = null, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string name, IAsyncEnumerable<ReadOnlyMemory<byte>> contents, Session session = null,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string name, ReadOnlyMemory<byte> content, Session session = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(int quantity, Session session = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names, Session session = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs, Session session = null,
        CancellationToken cancellationToken = default);

    Task<Blob> DownloadBlob(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunk(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunk(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);
}