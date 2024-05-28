using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    Task<BlobInfo> CreateBlobAsync(string sessionId, string name, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string sessionId, string name, IAsyncEnumerable<ReadOnlyMemory<byte>> contents,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string sessionId, string name, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(string sessionId, int quantity,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(string sessionId, IEnumerable<string> names,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(string sessionId,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
        CancellationToken cancellationToken = default);

    Task<Blob> DownloadBlob(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<byte[]> DownloadBlobAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    Task UploadBlobAsync(BlobInfo blobInfo, ReadOnlyMemory<byte> blobContent,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(BlobInfo blobInfo, IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task<BlobState> GetBlobStateAsync(BlobInfo blobInfo, CancellationToken cancellationToken = default);
}