using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.SortDirection;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
    IAsyncEnumerable<BlobInfo> CreateBlobsMetadataAsync(SessionInfo session, IEnumerable<string> names,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(SessionInfo session, string name, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task<BlobInfo> CreateBlobAsync(SessionInfo session, string name, IEnumerable<ReadOnlyMemory<byte>> contents,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<BlobInfo> CreateBlobsAsync(SessionInfo session,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadBlobAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<byte[]> DownloadBlobWithChunksAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default);

    Task UploadBlobChunkAsync(BlobInfo blobInfo, IEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken = default);

    Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default);

    Task<BlobState> GetBlobStateAsync(BlobInfo blobInfo, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BlobPage> ListBlobsAsync(BlobPagination blobPagination,
        CancellationToken cancellationToken = default);
}

public static class IBlobServiceExt
{
    public static IAsyncEnumerable<BlobInfo> CreateBlobsMetadataAsync(this IBlobService blobService,
        SessionInfo session,
        int quantity, CancellationToken cancellationToken = default)
    {
        return blobService.CreateBlobsMetadataAsync(session, Enumerable.Range(0, quantity)
            .Select(_ => Guid.NewGuid().ToString()).ToList(), cancellationToken);
    }

    public static async IAsyncEnumerable<BlobPage> ListAllBlobsAsync(this IBlobService blobService, SessionInfo session,
        int pageSize = 50, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var blobPagination = new BlobPagination
        {
            Filter = new Filters
            {
                Or =
                {
                    new FiltersAnd
                    {
                        And =
                        {
                            new FilterField
                            {
                                Field = new ResultField
                                {
                                    ResultRawField = new ResultRawField
                                    {
                                        Field = ResultRawEnumField.SessionId
                                    }
                                }
                            }
                        }
                    }
                }
            },
            Page = 0,
            PageSize = 50,
            SortDirection = (SortDirection)Enum.SortDirection.Asc
        };

        var total = 0;
        var firstPage = true;

        while (true)
        {
            await foreach (var blobPage in blobService.ListBlobsAsync(blobPagination, cancellationToken))
            {
                if (firstPage)
                {
                    total = blobPage.TotalPages;
                    firstPage = false;
                }

                yield return blobPage;
            }

            blobPagination.Page++;
            if (blobPagination.Page * pageSize >= total) break;
        }
    }
}