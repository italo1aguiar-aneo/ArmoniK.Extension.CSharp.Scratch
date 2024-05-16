using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.Client;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Extension.CSharp.Client.Common;
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
    public BlobService(ChannelBase channel, ILoggerFactory loggerFactory)
    {
        _blobClient = new ResultsClient(channel);
    }

    private readonly ResultsClient _blobClient;

    public async Task<BlobInfo> CreateBlobAsync(BlobInfo blobInfo, Session session,
        CancellationToken cancellationToken = default)
    {
        var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
        {
            Name = blobInfo.Name
        };
        var blobsCreationResponse =
            await _blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { taskCreation }
            });
        return new BlobInfo(blobsCreationResponse.Results.Single().Name,
            blobsCreationResponse.Results.Single().ResultId);
    }

    public async Task<BlobInfo> CreateBlobAsync(Blob blob, Session session,
        CancellationToken cancellationToken = default)
    {
        var blobMetadataCreationResponse = await _blobClient.CreateResultsMetaDataAsync(
            new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results =
                {
                    new CreateResultsMetaDataRequest.Types.ResultCreate
                    {
                        Name = blob.Name
                    }
                }
            }, cancellationToken: cancellationToken);

        using var uploadStream = _blobClient.UploadResultData();

        await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
        {
            DataChunk = ByteString.CopyFrom(blob.Content.Span),
            Id = new UploadResultDataRequest.Types.ResultIdentifier
            {
                ResultId = blobMetadataCreationResponse.Results.Single().ResultId,
                SessionId = session.Id
            }
        });

        return new BlobInfo(blob.Name, blobMetadataCreationResponse.Results.Single().ResultId);
    }

    public async Task<ICollection<BlobInfo>> CreateBlobsAsync(ICollection<BlobInfo> blobsInfos, Session session,
        CancellationToken cancellationToken = default)
    {
        var resultsCreate = new ConcurrentBag<CreateResultsMetaDataRequest.Types.ResultCreate>();
        Parallel.ForEach(blobsInfos, blobInfo =>
        {
            var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
            {
                Name = blobInfo.Name
            };
            resultsCreate.Add(taskCreation);
        });
        var blobsCreationResponse =
            await _blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate.ToList() }
            }, cancellationToken: cancellationToken);
        return new List<BlobInfo>(blobsCreationResponse.Results.Select(b => new BlobInfo(b.Name, b.ResultId)));
    }

    public async Task<ICollection<BlobInfo>> CreateBlobsAsync(ICollection<Blob> blobs, Session session,
        CancellationToken cancellationToken = default)
    {
        var resultsCreate = new ConcurrentBag<CreateResultsMetaDataRequest.Types.ResultCreate>();
        Parallel.ForEach(blobs, blob =>
        {
            var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
            {
                Name = blob.Name
            };
            resultsCreate.Add(taskCreation);
        });
        var blobsCreationResponse =
            await _blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
            {
                SessionId = session.Id,
                Results = { resultsCreate.ToList() }
            }, cancellationToken: cancellationToken);

        foreach (var blob in blobsCreationResponse.Results)
            blobs.Single(b => b.Name == blob.Name).SetBlobId(blob.ResultId);

        using var uploadStream = _blobClient.UploadResultData();

        var blobCreationTasks = blobs.Select(blob => Task.Run(async () =>
        {
            await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
            {
                DataChunk = ByteString.CopyFrom(blob.Content.Span),
                Id = new UploadResultDataRequest.Types.ResultIdentifier
                {
                    ResultId = blob.BlobId,
                    SessionId = session.Id
                }
            });
        }, cancellationToken));
        await Task.WhenAll(blobCreationTasks);
        return new List<BlobInfo>(blobs.Select(x => new BlobInfo(x.Name, x.BlobId)));
    }

    public async Task<Blob> DownloadBlob(BlobInfo blobInfo, Session session,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blob = new Blob(blobInfo.Name, blobInfo.BlobId);
            var data = await _blobClient.DownloadResultData(session.Id, blobInfo.BlobId, cancellationToken);
            blob.AddContent(data);
            return blob;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}