using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.Client;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Common.Exceptions;
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
    private ResultsServiceConfigurationResponse _serviceConfiguration;

    public BlobService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channelPool = channel;
        _logger = loggerFactory.CreateLogger<BlobService>();
    }

    private Session _session;
    public void SetSession(Session session)
    {
        _session = session;
    }

    private async Task LoadBlobServiceConfiguration(CancellationToken cancellationToken = default)
    {
        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        _serviceConfiguration = await blobClient.GetServiceConfigurationAsync(new Empty());
    }
    public async Task<BlobInfo> CreateBlobAsync(string name, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

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

    public async Task<BlobInfo> CreateBlobAsync(Session session = null, CancellationToken cancellationToken = default)
    {
        return await CreateBlobAsync(Guid.NewGuid().ToString(), session);
    }
    public async Task<BlobInfo> CreateBlobAsync(string name, IAsyncEnumerable<ReadOnlyMemory<byte>> contents, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var blobInfo = await CreateBlobAsync(name, session, cancellationToken);
        var blob = new Blob(blobInfo.Name, blobInfo.BlobId);
        await foreach (var content in contents.WithCancellation(cancellationToken))
        {
            blob.AddContent(content);
            await UploadBlobChunk(new List<Tuple<BlobInfo, ReadOnlyMemory<byte>>>() { new(blobInfo, content) }, session, cancellationToken: cancellationToken);
        }
        return new BlobInfo(name, blob.BlobId);

    }

    public async Task<BlobInfo> CreateBlobAsync(string name, ReadOnlyMemory<byte> content, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

        if(_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        if (_serviceConfiguration != null && content.Length > _serviceConfiguration.DataChunkMaxSize)
        {
            var blobInfo = await CreateBlobAsync(name, session, cancellationToken);
            var blob = new Blob(blobInfo.Name, blobInfo.BlobId);
            await UploadBlobChunk(new List<Tuple<BlobInfo,ReadOnlyMemory<byte>>>(){new(blobInfo, content)}, session, cancellationToken: cancellationToken);
            return new BlobInfo(name, blob.BlobId);
        }
        else
        {
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
    }

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(int quantity, Session session = null,
        CancellationToken cancellationToken = default)
    {
        return await CreateBlobsAsync(Enumerable.Range(0, quantity)
            .Select(_ => Guid.NewGuid().ToString()).ToList(), session);
    }

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<string> names, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

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

    public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);

        var tasks = blobKeyValuePairs.Select(blobKeyValuePair =>
            Task.Run(async () =>
            {
                var blobInfo = await CreateBlobAsync(blobKeyValuePair.Key, blobKeyValuePair.Value,session,cancellationToken);
                return blobInfo;
                
            }, cancellationToken)
        ).ToList();

        // Wait for all tasks to complete and gather results
        var blobCreationResponse = await Task.WhenAll(tasks);

        return blobCreationResponse.Select(x => new BlobInfo(x.Name, x.BlobId)).ToList();
    }

    public async Task<Blob> DownloadBlob(BlobInfo blobInfo, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

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

    public async Task UploadBlobChunk(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        try
        {
            foreach (var chunk in blobs.ToChunks(_serviceConfiguration.DataChunkMaxSize))
            {
                await UploadBlob(chunk, blobClient, session, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    public async Task UploadBlobChunk(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs, Session session = null,
        CancellationToken cancellationToken = default)
    {
        session ??= _session;

        if (session == null)
        {
            throw new UnsetSessionException();
        }

        if (_serviceConfiguration is null)
            await LoadBlobServiceConfiguration(cancellationToken);

        await using var channel = await _channelPool.GetAsync(cancellationToken).ConfigureAwait(false);
        var blobClient = new ResultsClient(channel);
        try
        {
            await foreach (var chunk in blobs.ToChunksAsync(_serviceConfiguration.DataChunkMaxSize,
                               Timeout.InfiniteTimeSpan, cancellationToken))
            {
                await UploadBlob(chunk, blobClient, session, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    private async Task UploadBlob(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs, ResultsClient blobClient, Session session,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var uploadStream = blobClient.UploadResultData();
            foreach (var (blobInfo, content) in blobs)
            {
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
            
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }
}