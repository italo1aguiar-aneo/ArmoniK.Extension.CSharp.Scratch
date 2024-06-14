using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class BlobHandler
{
    public readonly ArmoniKClient ArmoniKClient;
    public readonly BlobInfo BlobInfo;

    public BlobHandler(BlobInfo blobInfo, ArmoniKClient armoniKClient)
    {
        BlobInfo = blobInfo;
        ArmoniKClient = armoniKClient;
    }

    public BlobHandler(string blobName, string blobId, string sessionId, ArmoniKClient armoniKClient)
    {
        BlobInfo = new BlobInfo
        {
            BlobId = blobId,
            BlobName = blobName,
            SessionId = sessionId
        };
        ArmoniKClient = armoniKClient;
    }

    public async Task<BlobState> GetBlobStateAsync(CancellationToken cancellationToken = default)
    {
        var blobClient = await ArmoniKClient.GetBlobService();
        return await blobClient.GetBlobStateAsync(BlobInfo, cancellationToken);
    }

    public async IAsyncEnumerable<byte[]> DownloadBlobData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await foreach (var chunk in blobClient.DownloadBlobWithChunksAsync(BlobInfo, cancellationToken)
                           .ConfigureAwait(false))
            yield return chunk;
    }

    public async Task UploadBlobData(IEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await blobClient.UploadBlobChunkAsync(BlobInfo, blobContent, cancellationToken);
    }

    public async Task UploadBlobData(ReadOnlyMemory<byte> blobContent, CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        // Upload the blob chunk
        await blobClient.UploadBlobChunkAsync(BlobInfo, new[] { blobContent }, cancellationToken);
    }
}