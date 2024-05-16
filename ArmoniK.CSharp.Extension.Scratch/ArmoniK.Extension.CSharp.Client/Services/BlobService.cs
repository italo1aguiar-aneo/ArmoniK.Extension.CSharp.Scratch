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
        var blobCreationResponse = await _blobClient.CreateResultsAsync(
            new CreateResultsRequest
            {
                SessionId = session.Id,
                Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Name = blob.Name,
                        Data = ByteString.CopyFrom(blob.Content.Span)
                    }
                }
            }, cancellationToken: cancellationToken);

        return new BlobInfo(blob.Name, blobCreationResponse.Results.Single().ResultId);
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
        var resultsCreate = new ConcurrentBag<CreateResultsRequest.Types.ResultCreate>();
        Parallel.ForEach(blobs, blob =>
        {
            var taskCreation = new CreateResultsRequest.Types.ResultCreate
            {
                Name = blob.Name,
                Data = ByteString.CopyFrom(blob.Content.Span),
            };
            resultsCreate.Add(taskCreation);
        });
        var blobsCreationResponse =
            await _blobClient.CreateResultsAsync(new CreateResultsRequest
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