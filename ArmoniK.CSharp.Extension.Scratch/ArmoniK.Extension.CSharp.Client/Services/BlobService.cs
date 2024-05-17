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

    public BlobService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
    }

    public Task<BlobInfo> CreateBlobAsync(Session session, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var resultsCreate = new ConcurrentBag<CreateResultsMetaDataRequest.Types.ResultCreate>();

        Parallel.ForEach(names, blobName =>
        {
            var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
            {
                Name = blobName
            };
            resultsCreate.Add(taskCreation);
        });

        var blobsCreationResponse =
            await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate.ToList() }
            }, cancellationToken: cancellationToken);

        return new List<BlobInfo>(blobsCreationResponse.Results.Select(b => new BlobInfo(b.Name, b.ResultId)));
    }

    public Task<IEnumerable<BlobInfo>> CreateBlobsAsync(string quantity, Session session,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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

    public Task<Blob> DownloadBlob(string name, Session session, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
        } // see if we keep this and how to handle, should we throw ? 
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task<Blob> UpdateBlob(BlobInfo blobInfo, Session session, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}