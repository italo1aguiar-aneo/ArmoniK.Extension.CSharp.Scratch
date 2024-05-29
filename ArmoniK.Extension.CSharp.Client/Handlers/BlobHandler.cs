using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class BlobHandler : BlobInfo
{
    public readonly ArmoniKClient ArmoniKClient;

    public BlobHandler(BlobInfo blobInfo, ArmoniKClient armoniKClient) : base(blobInfo.Name, blobInfo.Id,
        blobInfo.SessionId)
    {
        ArmoniKClient = armoniKClient;
    }

    public BlobHandler(string name, string id, string sessionId, ArmoniKClient armoniKClient) : base(name, id,
        sessionId)
    {
        ArmoniKClient = armoniKClient;
    }

    public async Task<BlobState> GetBlobStateAsync(CancellationToken cancellationToken = default)
    {
        var blobClient = await ArmoniKClient.GetBlobService();
        return await blobClient.GetBlobStateAsync(this, cancellationToken);
    }

    public async IAsyncEnumerable<byte[]> DownloadBlobData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await foreach (var chunk in blobClient.DownloadBlobWithChunksAsync(this, cancellationToken).ConfigureAwait(false))
            yield return chunk;
    }

    public async Task UploadBlobData(IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await foreach (var chunk in blobContent.WithCancellation(cancellationToken))
            await blobClient.UploadBlobAsync(this, chunk, cancellationToken);
    }

    public async Task UploadBlobData(ReadOnlyMemory<byte> blobContent, CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();
        await blobClient.UploadBlobAsync(this, blobContent, cancellationToken);
    }
}