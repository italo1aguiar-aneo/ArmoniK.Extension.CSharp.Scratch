using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

/// <summary>
///     Provides methods for handling operations related to blobs, such as retrieving state, downloading, and uploading
///     blob data.
/// </summary>
public class BlobHandler
{
    /// <summary>
    ///     The ArmoniK client used for performing blob operations.
    /// </summary>
    public readonly ArmoniKClient ArmoniKClient;

    /// <summary>
    ///     Information about the blob being handled.
    /// </summary>
    public readonly BlobInfo BlobInfo;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlobHandler" /> class with specified blob information and an ArmoniK
    ///     client.
    /// </summary>
    /// <param name="blobInfo">The information about the blob.</param>
    /// <param name="armoniKClient">The ArmoniK client used for performing blob operations.</param>
    public BlobHandler(BlobInfo blobInfo, ArmoniKClient armoniKClient)
    {
        BlobInfo = blobInfo;
        ArmoniKClient = armoniKClient;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlobHandler" /> class with specified blob details and an ArmoniK
    ///     client.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="blobId">The identifier of the blob.</param>
    /// <param name="sessionId">The session identifier associated with the blob.</param>
    /// <param name="armoniKClient">The ArmoniK client used for performing blob operations.</param>
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

    /// <summary>
    ///     Asynchronously retrieves the state of the blob.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the blob state.</returns>
    public async Task<BlobState> GetBlobStateAsync(CancellationToken cancellationToken = default)
    {
        var blobClient = await ArmoniKClient.GetBlobService();
        return await blobClient.GetBlobStateAsync(BlobInfo, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously downloads the data of the blob in chunks.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An asynchronous enumerable of byte arrays representing the blob data chunks.</returns>
    public async IAsyncEnumerable<byte[]> DownloadBlobData([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await foreach (var chunk in blobClient.DownloadBlobWithChunksAsync(BlobInfo, cancellationToken)
                           .ConfigureAwait(false))
            yield return chunk;
    }

    /// <summary>
    ///     Asynchronously uploads the specified content chunks to the blob.
    /// </summary>
    /// <param name="blobContent">The content chunks to upload to the blob.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UploadBlobData(IEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        await blobClient.UploadBlobChunkAsync(BlobInfo, blobContent, cancellationToken);
    }

    /// <summary>
    ///     Asynchronously uploads the specified content to the blob.
    /// </summary>
    /// <param name="blobContent">The content to upload to the blob.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UploadBlobData(ReadOnlyMemory<byte> blobContent, CancellationToken cancellationToken)
    {
        var blobClient = await ArmoniKClient.GetBlobService();

        // Upload the blob chunk
        await blobClient.UploadBlobChunkAsync(BlobInfo, new[] { blobContent }, cancellationToken);
    }
}