// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2024. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

public class BlobService : IBlobService
{
  private readonly ObjectPool<ChannelBase> channelPool_;
  private readonly ILogger<BlobService>    logger_;

  private ResultsServiceConfigurationResponse serviceConfiguration_;

  public BlobService(ObjectPool<ChannelBase> channel,
                     ILoggerFactory          loggerFactory)
  {
    channelPool_ = channel;
    logger_      = loggerFactory.CreateLogger<BlobService>();
  }

  public async Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo       session,
                                                      string            name,
                                                      CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    var taskCreation = new CreateResultsMetaDataRequest.Types.ResultCreate
                       {
                         Name = name,
                       };
    var blobsCreationResponse = await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
                                                                            {
                                                                              SessionId = session.SessionId,
                                                                              Results =
                                                                              {
                                                                                taskCreation,
                                                                              },
                                                                            });
    return new BlobInfo
           {
             BlobName = blobsCreationResponse.Results.Single()
                                             .Name,

             BlobId = blobsCreationResponse.Results.Single()
                                           .ResultId,
             SessionId = blobsCreationResponse.Results.Single()
                                              .SessionId,
           };
  }

  public async Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo       session,
                                                                    int               quantity,
                                                                    CancellationToken cancellationToken = default)
    => await CreateBlobsMetadataAsync(session,
                                      Enumerable.Range(0,
                                                       quantity)
                                                .Select(_ => Guid.NewGuid()
                                                                 .ToString())
                                                .ToList(),
                                      cancellationToken);

  public async Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo         session,
                                                                    IEnumerable<string> names,
                                                                    CancellationToken   cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);

    var resultsCreate = names.Select(blobName => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                 {
                                                   Name = blobName,
                                                 })
                             .ToList();

    var blobsCreationResponse = await blobClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
                                                                            {
                                                                              SessionId = session.SessionId,
                                                                              Results =
                                                                              {
                                                                                resultsCreate,
                                                                              },
                                                                            },
                                                                            cancellationToken: cancellationToken);

    return new List<BlobInfo>(blobsCreationResponse.Results.Select(b => new BlobInfo
                                                                        {
                                                                          BlobName  = b.Name,
                                                                          BlobId    = b.ResultId,
                                                                          SessionId = session.SessionId,
                                                                        }));
  }

  public async Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo       session,
                                                      CancellationToken cancellationToken = default)
    => await CreateBlobMetadataAsync(session,
                                     Guid.NewGuid()
                                         .ToString(),
                                     cancellationToken);

  public async Task<BlobInfo> CreateBlobAsync(SessionInfo          session,
                                              string               name,
                                              ReadOnlyMemory<byte> content,
                                              CancellationToken    cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);

    if (serviceConfiguration_ != null && content.Length > serviceConfiguration_.DataChunkMaxSize)
    {
      var blobInfoEmpty = await CreateEmptyBlobAsync(session,
                                                     name,
                                                     blobClient,
                                                     cancellationToken);

      await UploadBlobsAsync(new List<Tuple<BlobInfo, ReadOnlyMemory<byte>>>
                             {
                               new(blobInfoEmpty,
                                   content),
                             },
                             blobClient,
                             cancellationToken);
      return blobInfoEmpty;
    }

    var blobCreationResponse = await blobClient.CreateResultsAsync(new CreateResultsRequest
                                                                   {
                                                                     SessionId = session.SessionId,
                                                                     Results =
                                                                     {
                                                                       new CreateResultsRequest.Types.ResultCreate
                                                                       {
                                                                         Name = name,
                                                                       },
                                                                     },
                                                                   },
                                                                   cancellationToken: cancellationToken);

    return new BlobInfo
           {
             BlobName = name,
             BlobId = blobCreationResponse.Results.Single()
                                          .ResultId,
             SessionId = session.SessionId,
           };
  }

  public async Task<BlobInfo> CreateBlobFromChunksAsync(SessionInfo                            session,
                                                        string                                 name,
                                                        IAsyncEnumerable<ReadOnlyMemory<byte>> contents,
                                                        CancellationToken                      cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);

    var blobInfo = await CreateEmptyBlobAsync(session,
                                              name,
                                              blobClient,
                                              cancellationToken);
    await foreach (var content in contents.WithCancellation(cancellationToken))
    {
      await UploadBlobsAsync(new List<Tuple<BlobInfo, ReadOnlyMemory<byte>>>
                             {
                               new(blobInfo,
                                   content),
                             },
                             blobClient,
                             cancellationToken);
    }

    return new BlobInfo
           {
             BlobName  = name,
             BlobId    = blobInfo.BlobId,
             SessionId = session.SessionId,
           };
  }

  public async Task<IEnumerable<BlobInfo>> CreateBlobsAsync(SessionInfo                                             session,
                                                            IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
                                                            CancellationToken                                       cancellationToken = default)
  {
    var tasks = blobKeyValuePairs.Select(blobKeyValuePair => Task.Run(async () =>
                                                                      {
                                                                        var blobInfo = await CreateBlobAsync(session,
                                                                                                             blobKeyValuePair.Key,
                                                                                                             blobKeyValuePair.Value,
                                                                                                             cancellationToken);
                                                                        return blobInfo;
                                                                      },
                                                                      cancellationToken))
                                 .ToList();

    // Wait for all tasks to complete and gather results
    var blobCreationResponse = await Task.WhenAll(tasks);

    return blobCreationResponse.Select(x => new BlobInfo
                                            {
                                              BlobName  = x.BlobName,
                                              BlobId    = x.BlobId,
                                              SessionId = session.SessionId,
                                            })
                               .ToList();
  }

  public async Task UploadBlobChunkAsync(BlobInfo                               blobInfo,
                                         IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
                                         CancellationToken                      cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    try
    {
      await foreach (var chunk in blobContent.ToChunksAsync(serviceConfiguration_.DataChunkMaxSize,
                                                            Timeout.InfiniteTimeSpan,
                                                            cancellationToken))
      {
        await UploadBlob(blobInfo,
                         chunk,
                         blobClient);
      }
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  public async Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                                     CancellationToken                                  cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);

    await UploadBlobsAsync(blobs,
                           blobClient,
                           cancellationToken);
  }

  public async Task UploadBlobsChunksAsync(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                                           CancellationToken                                       cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    try
    {
      await foreach (var chunk in blobs.ToChunksAsync(serviceConfiguration_.DataChunkMaxSize,
                                                      Timeout.InfiniteTimeSpan,
                                                      cancellationToken))
      {
        await UploadBlob(chunk,
                         blobClient);
      }
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  public async Task UploadBlobAsync(BlobInfo             blobInfo,
                                    ReadOnlyMemory<byte> blobContent,
                                    CancellationToken    cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    try
    {
      using var uploadStream = blobClient.UploadResultData();
      await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                                                  {
                                                    DataChunk = ByteString.CopyFrom(blobContent.Span),
                                                    Id = new UploadResultDataRequest.Types.ResultIdentifier
                                                         {
                                                           ResultId  = blobInfo.BlobId,
                                                           SessionId = blobInfo.SessionId,
                                                         },
                                                  });
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  public async Task<byte[]> DownloadBlobAsync(BlobInfo          blobInfo,
                                              CancellationToken cancellationToken = default)
  {
    try
    {
      await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                  .ConfigureAwait(false);
      var blobClient = new ResultsClient(channel);
      return await blobClient.DownloadResultData(blobInfo.SessionId,
                                                 blobInfo.BlobId,
                                                 cancellationToken);
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  public async IAsyncEnumerable<byte[]> DownloadBlobWithChunksAsync(BlobInfo                                   blobInfo,
                                                                    [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    var stream = blobClient.DownloadResultData(new DownloadResultDataRequest
                                               {
                                                 ResultId  = blobInfo.BlobId,
                                                 SessionId = blobInfo.SessionId,
                                               },
                                               cancellationToken: cancellationToken);
    while (await stream.ResponseStream.MoveNext(cancellationToken))
    {
      yield return stream.ResponseStream.Current.DataChunk.ToByteArray();
    }
  }

  public async Task<BlobState> GetBlobStateAsync(BlobInfo          blobInfo,
                                                 CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    var blobDetails = await blobClient.GetResultAsync(new GetResultRequest
                                                      {
                                                        ResultId = blobInfo.BlobId,
                                                      });
    return new BlobState
           {
             CreateAt    = blobDetails.Result.CreatedAt.ToDateTime(),
             CompletedAt = blobDetails.Result.CompletedAt.ToDateTime(),
             Status      = (BlobStatus?)blobDetails.Result.Status,
             BlobId      = blobDetails.Result.ResultId,
             SessionId   = blobDetails.Result.SessionId,
             BlobName    = blobDetails.Result.Name,
           };
  }

  public async Task<IEnumerable<BlobState>> ListBlobsAsync(BlobPagination    blobPagination,
                                                           CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);
    var listResultsResponse = await blobClient.ListResultsAsync(new ListResultsRequest
                                                                {
                                                                  Sort = new ListResultsRequest.Types.Sort
                                                                         {
                                                                           Direction = blobPagination.SortDirection,
                                                                         },
                                                                  Filters  = blobPagination.Filter, // should have sessionId filter... see how it should be implemented
                                                                  Page     = blobPagination.Page,
                                                                  PageSize = blobPagination.PageSize,
                                                                });
    return listResultsResponse.Results.Select(x => new BlobState
                                                   {
                                                     CreateAt    = x.CreatedAt.ToDateTime(),
                                                     CompletedAt = x.CompletedAt.ToDateTime(),
                                                     Status      = (BlobStatus?)x.Status,
                                                     BlobId      = x.ResultId,
                                                     BlobName    = x.Name,
                                                     SessionId   = x.SessionId,
                                                   });
  }

  private async Task<BlobInfo> CreateEmptyBlobAsync(SessionInfo       session,
                                                    string            name,
                                                    ResultsClient     blobClient,
                                                    CancellationToken cancellationToken)
  {
    var blobCreationResponse = await blobClient.CreateResultsAsync(new CreateResultsRequest
                                                                   {
                                                                     SessionId = session.SessionId,
                                                                     Results =
                                                                     {
                                                                       new CreateResultsRequest.Types.ResultCreate
                                                                       {
                                                                         Name = name,
                                                                       },
                                                                     },
                                                                   },
                                                                   cancellationToken: cancellationToken);
    return new BlobInfo
           {
             BlobName = name,
             BlobId = blobCreationResponse.Results.Single()
                                          .ResultId,
             SessionId = session.SessionId,
           };
  }

  private async Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                                      ResultsClient                                      blobClient,
                                      CancellationToken                                  cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    try
    {
      foreach (var chunk in blobs.ToChunks(serviceConfiguration_.DataChunkMaxSize))
      {
        await UploadBlob(chunk,
                         blobClient);
      }
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  private async Task LoadBlobServiceConfiguration(CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new ResultsClient(channel);

    serviceConfiguration_ = await blobClient.GetServiceConfigurationAsync(new Empty());
  }

  private async Task UploadBlob(BlobInfo                          blob,
                                IEnumerable<ReadOnlyMemory<byte>> blobContent,
                                ResultsClient                     blobClient)
  {
    try
    {
      using var uploadStream = blobClient.UploadResultData();
      foreach (var content in blobContent)
      {
        await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                                                    {
                                                      DataChunk = ByteString.CopyFrom(content.Span),
                                                      Id = new UploadResultDataRequest.Types.ResultIdentifier
                                                           {
                                                             ResultId  = blob.BlobId,
                                                             SessionId = blob.SessionId,
                                                           },
                                                    });
      }
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }

  private async Task UploadBlob(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                                ResultsClient                                      blobClient)
  {
    try
    {
      using var uploadStream = blobClient.UploadResultData();
      foreach (var blob in blobs)
      {
        await uploadStream.RequestStream.WriteAsync(new UploadResultDataRequest
                                                    {
                                                      DataChunk = ByteString.CopyFrom(blob.Item2.Span),
                                                      Id = new UploadResultDataRequest.Types.ResultIdentifier
                                                           {
                                                             ResultId  = blob.Item1.BlobId,
                                                             SessionId = blob.Item1.SessionId,
                                                           },
                                                    });
      }
    }
    catch (Exception e)
    {
      logger_.LogError(e.Message);
      throw;
    }
  }
}
