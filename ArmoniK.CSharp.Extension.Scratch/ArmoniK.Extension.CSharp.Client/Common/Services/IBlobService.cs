using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    Task<BlobInfo> CreateBlobAsync(string name, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string name, IAsyncEnumerable<ReadOnlyMemory<byte>> contents,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(string name, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(int quantity,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
        CancellationToken cancellationToken = default);

    Task<Blob> DownloadBlob(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunk(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunk(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);
}