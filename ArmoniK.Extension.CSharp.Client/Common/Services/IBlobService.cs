using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo session, string name, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo session, CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobFromChunksAsync(SessionInfo session, string name, IAsyncEnumerable<ReadOnlyMemory<byte>> contents,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(SessionInfo session, string name, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo session, int quantity,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo session, IEnumerable<string> names,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobInfo>> CreateBlobsAsync(SessionInfo session,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadBlobAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<byte[]> DownloadBlobWithChunksAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    Task UploadBlobAsync(BlobInfo blobInfo, ReadOnlyMemory<byte> blobContent,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(BlobInfo blobInfo, IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken = default);

    Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task<BlobState> GetBlobStateAsync(BlobInfo blobInfo, CancellationToken cancellationToken = default);

    Task<IEnumerable<BlobState>> ListBlobsAsync(BlobPagination blobPagination,
        CancellationToken cancellationToken = default);
}