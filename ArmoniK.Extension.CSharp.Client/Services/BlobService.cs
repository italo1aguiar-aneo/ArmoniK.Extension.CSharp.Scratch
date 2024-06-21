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
using ArmoniK.Extension.CSharp.Client.Common.Enum;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;

using Google.Protobuf;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class BlobService : IBlobService
{
  private readonly ObjectPool<ChannelBase>             channelPool_;
  private readonly ILogger<BlobService>                logger_;
  private          ResultsServiceConfigurationResponse serviceConfiguration_;

  public BlobService(ObjectPool<ChannelBase> channel,
                     ILoggerFactory          loggerFactory)
  {
    channelPool_ = channel;
    logger_      = loggerFactory.CreateLogger<BlobService>();
  }

  public async IAsyncEnumerable<BlobInfo> CreateBlobsMetadataAsync(SessionInfo                                session,
                                                                   IEnumerable<string>                        names,
                                                                   [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new Results.ResultsClient(channel);

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

    var asyncBlobInfos = blobsCreationResponse.Results.Select(b => new BlobInfo
                                                                   {
                                                                     BlobName  = b.Name,
                                                                     BlobId    = b.ResultId,
                                                                     SessionId = session.SessionId,
                                                                   })
                                              .ToAsyncEnumerable();

    await foreach (var blobInfo in asyncBlobInfos.WithCancellation(cancellationToken))
    {
      yield return blobInfo;
    }
  }


  public async Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                                     CancellationToken                                  cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new Results.ResultsClient(channel);

    await UploadBlobsAsync(blobs,
                           blobClient,
                           cancellationToken);
  }

  public async Task<byte[]> DownloadBlobAsync(BlobInfo          blobInfo,
                                              CancellationToken cancellationToken = default)
  {
    try
    {
      await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                  .ConfigureAwait(false);
      var blobClient = new Results.ResultsClient(channel);
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
    var blobClient = new Results.ResultsClient(channel);
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
    var blobClient = new Results.ResultsClient(channel);
    var blobDetails = await blobClient.GetResultAsync(new GetResultRequest
                                                      {
                                                        ResultId = blobInfo.BlobId,
                                                      });
    return new BlobState
           {
             CreateAt    = blobDetails.Result.CreatedAt.ToDateTime(),
             CompletedAt = blobDetails.Result.CompletedAt.ToDateTime(),
             Status      = blobDetails.Result.Status.ToInternalStatus(),
             BlobId      = blobDetails.Result.ResultId,
             SessionId   = blobDetails.Result.SessionId,
             BlobName    = blobDetails.Result.Name,
           };
  }

  public async Task<BlobInfo> CreateBlobAsync(SessionInfo                       session,
                                              string                            name,
                                              IEnumerable<ReadOnlyMemory<byte>> contents,
                                              CancellationToken                 cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new Results.ResultsClient(channel);

    var blobContent = contents as ReadOnlyMemory<byte>[] ?? contents.ToArray();

    var blobInfo = await CreateEmptyBlobAsync(session,
                                              name,
                                              blobClient,
                                              cancellationToken);
    await UploadBlobChunkAsync(blobInfo,
                               blobContent,
                               cancellationToken);
    return blobInfo;
  }

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
    var blobClient = new Results.ResultsClient(channel);

    if (serviceConfiguration_ != null && content.Length > serviceConfiguration_.DataChunkMaxSize)
    {
      return await CreateBlobAsync(session,
                                   name,
                                   new[]
                                   {
                                     content,
                                   },
                                   cancellationToken);
    }

    var blobCreationResponse = await blobClient.CreateResultsAsync(new CreateResultsRequest
                                                                   {
                                                                     SessionId = session.SessionId,
                                                                     Results =
                                                                     {
                                                                       new CreateResultsRequest.Types.ResultCreate
                                                                       {
                                                                         Name = name,
                                                                         Data = ByteString.CopyFrom(content.Span),
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

  public async IAsyncEnumerable<BlobInfo> CreateBlobsAsync(SessionInfo                                             session,
                                                           IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
                                                           [EnumeratorCancellation] CancellationToken              cancellationToken = default)
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

    var blobCreationResponse = await Task.WhenAll(tasks);

    foreach (var blob in blobCreationResponse)
    {
      yield return new BlobInfo
                   {
                     BlobName  = blob.BlobName,
                     BlobId    = blob.BlobId,
                     SessionId = session.SessionId,
                   };
    }
  }

  public async Task UploadBlobChunkAsync(BlobInfo                          blobInfo,
                                         IEnumerable<ReadOnlyMemory<byte>> blobContent,
                                         CancellationToken                 cancellationToken = default)
  {
    if (serviceConfiguration_ is null)
    {
      await LoadBlobServiceConfiguration(cancellationToken);
    }

    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new Results.ResultsClient(channel);
    try
    {
      foreach (var chunk in blobContent.ToChunks(serviceConfiguration_.DataChunkMaxSize))
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

  public async IAsyncEnumerable<BlobPage> ListBlobsAsync(BlobPagination                             blobPagination,
                                                         [EnumeratorCancellation] CancellationToken cancellationToken = default)
  {
    await using var channel = await channelPool_.GetAsync(cancellationToken)
                                                .ConfigureAwait(false);
    var blobClient = new Results.ResultsClient(channel);
    var listResultsResponse = await blobClient.ListResultsAsync(new ListResultsRequest
                                                                {
                                                                  Sort = new ListResultsRequest.Types.Sort
                                                                         {
                                                                           Direction = blobPagination.SortDirection.ToGrpc(),
                                                                         },
                                                                  Filters  = blobPagination.Filter,
                                                                  Page     = blobPagination.Page,
                                                                  PageSize = blobPagination.PageSize,
                                                                },
                                                                cancellationToken: cancellationToken);
    foreach (var x in listResultsResponse.Results)
    {
      yield return new BlobPage
                   {
                     TotalPages = listResultsResponse.Total,
                     BlobDetails = new BlobState
                                   {
                                     CreateAt    = x.CreatedAt.ToDateTime(),
                                     CompletedAt = x.CompletedAt?.ToDateTime(),
                                     Status      = x.Status.ToInternalStatus(),
                                     BlobId      = x.ResultId,
                                     BlobName    = x.Name,
                                     SessionId   = x.SessionId,
                                   },
                   };
    }
  }


  private async Task<BlobInfo> CreateEmptyBlobAsync(SessionInfo           session,
                                                    string                name,
                                                    Results.ResultsClient blobClient,
                                                    CancellationToken     cancellationToken)
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
                                      Results.ResultsClient                              blobClient,
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
    var blobClient = new Results.ResultsClient(channel);
    serviceConfiguration_ = await blobClient.GetServiceConfigurationAsync(new Empty());
  }

  private async Task UploadBlob(BlobInfo                          blob,
                                IEnumerable<ReadOnlyMemory<byte>> blobContent,
                                Results.ResultsClient             blobClient)
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
                                Results.ResultsClient                              blobClient)
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
