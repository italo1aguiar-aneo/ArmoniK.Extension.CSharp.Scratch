using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.Client;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static ArmoniK.Api.gRPC.V1.Results.Results;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class BlobService : IBlobService
{
    private readonly ObjectPool<ChannelBase> _channelPool;
    private readonly ILogger<BlobService> _logger;

    public BlobService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<BlobService>();
    }

    public async Task<BlobInfo> CreateBlobAsync(Session session, CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
        {
            Name = Guid.NewGuid().ToString()
        };
        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { taskCreation }
            });
        return new BlobInfo(blobsCreationResponse.Results.Single().Name,
            blobsCreationResponse.Results.Single().ResultId);
    }

    public async Task<BlobInfo> CreateBlobAsync(string name, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
        {
            Name = name
        };
        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { taskCreation }
            });
        return new BlobInfo(blobsCreationResponse.Results.Single().Name,
            blobsCreationResponse.Results.Single().ResultId);
    }

    public async Task<BlobInfo> CreateBlobAsync(string name, ReadOnlyMemory<byte> content, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        var blobCreationResponse = await blobClient.CreateResultsAsync(
            new CreateResultsRequest
            {
                SessionId = session.Id,
                Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Name = name,
                        Data = ByteString.CopyFrom(content.Span)
                    }
                }
            }, cancellationToken: cancellationToken);

        return new BlobInfo(name, blobCreationResponse.Results.Single().ResultId);
    }

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(int quantity, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var resultsCreate = Enumerable.Range(0, quantity).Select(_ =>
            new CreateResultsMetaDataRequest.Types.ResultCreate { Name = new Guid().ToString() }).ToList();

        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate }
            }, cancellationToken: cancellationToken);

        return new List<BlobInfo>(blobsCreationResponse.Results.Select(b => new BlobInfo(b.Name, b.ResultId)));
    }

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var resultsCreate = names
            .Select(blobName => new CreateResultsMetaDataRequest.Types.ResultCreate { Name = blobName }).ToList();

        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate }
            }, cancellationToken: cancellationToken);

        return new List<BlobInfo>(blobsCreationResponse.Results.Select(b => new BlobInfo(b.Name, b.ResultId)));
    }

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names,
        IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var resultsCreate = new ConcurrentBag<CreateResultsRequest.Types.ResultCreate>();
        Parallel.ForEach(blobKeyValuePairs, blob =>
        {
            var taskCreation = new CreateResultsRequest.Types.ResultCreate
            {
                Name = blob.Key,
                Data = ByteString.CopyFrom(blob.Value.Span)
            };
            resultsCreate.Add(taskCreation);
        });
        var blobsCreationResponse =
            await blobClient.CreateResultsAsync(new CreateResultsRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate.ToList() }
            }, cancellationToken: cancellationToken);

        return blobsCreationResponse.Results.Select(x => new BlobInfo(x.Name, x.ResultId)).ToList();
    }

    public async Task<Blob> DownloadBlob(BlobInfo blobInfo, Session session,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
            var blobClient = new ResultsClient(channel);
            var blob = new Blob(blobInfo.Name, blobInfo.BlobId);
            var data = await blobClient.DownloadResultData(session.Id, blobInfo.BlobId, cancellationToken);
            blob.AddContent(data);
            return blob;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    //à voir si on a besoin des chuncks
    public async Task UploadBlob(BlobInfo blobInfo, ReadOnlyMemory<byte> content, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        try
        {
            using var uploadStream = blobClient.UploadResultData();
            await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
            {
                DataChunk = ByteString.CopyFrom(content.Span),
                Id = new UploadResultDataRequest.Types.ResultIdentifier
                {
                    ResultId = blobInfo.BlobId,
                    SessionId = session.Id
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