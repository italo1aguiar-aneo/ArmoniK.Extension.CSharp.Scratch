using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.Client;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ArmoniK.Api.gRPC.V1.Results.Results;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class BlobService : IBlobService
{
    private readonly ObjectPool<ChannelBase> _channelPool;
    private readonly ILogger<BlobService> _logger;

    private ResultsServiceConfigurationResponse _serviceConfiguration;

    public BlobService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<BlobService>();
    }

    public async IAsyncEnumerable<BlobInfo> CreateBlobsMetadataAsync(SessionInfo session, IEnumerable<string> names,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var resultsCreate = names
            .Select(blobName => new CreateResultsMetaDataRequest.Types.ResultCreate { Name = blobName }).ToList();

        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.SessionId,
                Results = { resultsCreate }
            }, cancellationToken: cancellationToken);

        var asyncBlobInfos = blobsCreationResponse.Results.Select(b => new BlobInfo
        {
            BlobName = b.Name,
            BlobId = b.ResultId,
            SessionId = session.SessionId
        }).ToAsyncEnumerable();

        await foreach (var blobInfo in asyncBlobInfos.WithCancellation(cancellationToken)) yield return blobInfo;
    }


    public async Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        await UploadBlobsAsync(blobs, blobClient, cancellationToken);
    }

    public async Task<byte[]> DownloadBlobAsync(BlobInfo blobInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = new ResultsClient(channel);
            return await blobClient.DownloadResultData(blobInfo.SessionId, blobInfo.BlobId, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<byte[]> DownloadBlobWithChunksAsync(BlobInfo blobInfo,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var stream = blobClient.DownloadResultData(
            new DownloadResultDataRequest { ResultId = blobInfo.BlobId, SessionId = blobInfo.SessionId },
            cancellationToken: cancellationToken);
        while (await stream.ResponseStream.MoveNext(cancellationToken))
            yield return stream.ResponseStream.Current.DataChunk.ToByteArray();
    }

    public async Task<BlobState> GetBlobStateAsync(BlobInfo blobInfo, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var blobDetails = await blobClient.GetResultAsync(new GetResultRequest
        {
            ResultId = blobInfo.BlobId
        });
        return new BlobState
        {
            CreateAt = blobDetails.Result.CreatedAt.ToDateTime(),
            CompletedAt = blobDetails.Result.CompletedAt.ToDateTime(),
            Status = (BlobStatus)blobDetails.Result.Status,
            BlobId = blobDetails.Result.ResultId,
            SessionId = blobDetails.Result.SessionId,
            BlobName = blobDetails.Result.Name
        };
    }

    public async Task<BlobInfo> CreateBlobAsync(SessionInfo session, string name,
        IEnumerable<ReadOnlyMemory<byte>> contents, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var blobContent = contents as ReadOnlyMemory<byte>[] ?? contents.ToArray();

        var blobInfo = await CreateEmptyBlobAsync(session, name, blobClient, cancellationToken);
        await UploadBlobChunkAsync(blobInfo, blobContent, cancellationToken);
        return blobInfo;
    }

    public async Task<BlobInfo> CreateBlobAsync(SessionInfo session, string name,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        if (_serviceConfiguration != null && content.Length > _serviceConfiguration.DataChunkMaxSize)
            return await CreateBlobAsync(session, name, new[] { content }, cancellationToken);

        var blobCreationResponse = await blobClient.CreateResultsAsync(
            new CreateResultsRequest
            {
                SessionId = session.SessionId,
                Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Name = name,
                        Data = ByteString.CopyFrom(content.Span)
                    }
                }
            }, cancellationToken: cancellationToken);

        return new BlobInfo
        {
            BlobName = name,
            BlobId = blobCreationResponse.Results.Single().ResultId,
            SessionId = session.SessionId
        };
    }

    public async IAsyncEnumerable<BlobInfo> CreateBlobsAsync(SessionInfo session,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var tasks = blobKeyValuePairs.Select(blobKeyValuePair =>
            Task.Run(async () =>
            {
                var blobInfo = await CreateBlobAsync(session, blobKeyValuePair.Key, blobKeyValuePair.Value,
                    cancellationToken);
                return blobInfo;
            }, cancellationToken)
        ).ToList();

        var blobCreationResponse = await Task.WhenAll(tasks);

        foreach (var blob in blobCreationResponse)
            yield return new BlobInfo
            {
                BlobName = blob.BlobName,
                BlobId = blob.BlobId,
                SessionId = session.SessionId
            };
    }

    public async Task UploadBlobChunkAsync(BlobInfo blobInfo, IEnumerable<ReadOnlyMemory<byte>> blobContent,
        CancellationToken cancellationToken = default)
    {
        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        try
        {
            foreach (var chunk in blobContent.ToChunks(_serviceConfiguration.DataChunkMaxSize))
                await UploadBlob(blobInfo, chunk, blobClient);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<BlobPage> ListBlobsAsync(BlobPagination blobPagination,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var listResultsResponse = await blobClient.ListResultsAsync(
            new ListResultsRequest
            {
                Sort = new ListResultsRequest.Types.Sort { Direction = blobPagination.SortDirection },
                Filters = blobPagination.Filter,
                Page = blobPagination.Page,
                PageSize = blobPagination.PageSize
            },
            cancellationToken: cancellationToken
        );
        foreach (var x in listResultsResponse.Results)
            yield return new BlobPage
            {
                TotalPages = listResultsResponse.Total,
                BlobDetails = new BlobState
                {
                    CreateAt = x.CreatedAt.ToDateTime(),
                    CompletedAt = x.CompletedAt?.ToDateTime(),
                    Status = (BlobStatus)x.Status,
                    BlobId = x.ResultId,
                    BlobName = x.Name,
                    SessionId = x.SessionId
                }
            };
    }


    private async Task<BlobInfo> CreateEmptyBlobAsync(SessionInfo session, string name, ResultsClient blobClient,
        CancellationToken cancellationToken)
    {
        var blobCreationResponse = await blobClient.CreateResultsAsync(
            new CreateResultsRequest
            {
                SessionId = session.SessionId,
                Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Name = name
                    }
                }
            }, cancellationToken: cancellationToken);
        return new BlobInfo
        {
            BlobName = name,
            BlobId = blobCreationResponse.Results.Single().ResultId,
            SessionId = session.SessionId
        };
    }

    private async Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        ResultsClient blobClient,
        CancellationToken cancellationToken = default)
    {
        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        try
        {
            foreach (var chunk in blobs.ToChunks(_serviceConfiguration.DataChunkMaxSize))
                await UploadBlob(chunk, blobClient);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    private async Task LoadBlobServiceConfiguration(CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        _serviceConfiguration = await blobClient.GetServiceConfigurationAsync(new Empty());
    }

    private async Task UploadBlob(BlobInfo blob, IEnumerable<ReadOnlyMemory<byte>> blobContent,
        ResultsClient blobClient)
    {
        try
        {
            using var uploadStream = blobClient.UploadResultData();
            foreach (var content in blobContent)
                await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                {
                    DataChunk = ByteString.CopyFrom(content.Span),
                    Id = new UploadResultDataRequest.Types.ResultIdentifier
                    {
                        ResultId = blob.BlobId,
                        SessionId = blob.SessionId
                    }
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    private async Task UploadBlob(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
        ResultsClient blobClient)
    {
        try
        {
            using var uploadStream = blobClient.UploadResultData();
            foreach (var blob in blobs)
                await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                {
                    DataChunk = ByteString.CopyFrom(blob.Item2.Span),
                    Id = new UploadResultDataRequest.Types.ResultIdentifier
                    {
                        ResultId = blob.Item1.BlobId,
                        SessionId = blob.Item1.SessionId
                    }
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}