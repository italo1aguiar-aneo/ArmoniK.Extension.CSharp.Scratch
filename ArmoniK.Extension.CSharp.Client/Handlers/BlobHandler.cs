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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class BlobHandler
{
  public readonly ArmoniKClient ArmoniKClient;
  public readonly BlobInfo      BlobInfo;

  public BlobHandler(BlobInfo      blobInfo,
                     ArmoniKClient armoniKClient)
  {
    BlobInfo      = blobInfo;
    ArmoniKClient = armoniKClient;
  }

  public BlobHandler(string        blobName,
                     string        blobId,
                     string        sessionId,
                     ArmoniKClient armoniKClient)
  {
    BlobInfo = new BlobInfo
               {
                 BlobId    = blobId,
                 BlobName  = blobName,
                 SessionId = sessionId,
               };
    ArmoniKClient = armoniKClient;
  }

  public async Task<BlobState> GetBlobStateAsync(CancellationToken cancellationToken = default)
  {
    var blobClient = await ArmoniKClient.GetBlobService();
    return await blobClient.GetBlobStateAsync(BlobInfo,
                                              cancellationToken);
  }


  public async IAsyncEnumerable<byte[]> DownloadBlobData([EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var blobClient = await ArmoniKClient.GetBlobService();

    await foreach (var chunk in blobClient.DownloadBlobWithChunksAsync(BlobInfo,
                                                                       cancellationToken)
                                          .ConfigureAwait(false))
    {
      yield return chunk;
    }
  }

  public async Task UploadBlobData(IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
                                   CancellationToken                      cancellationToken)
  {
    var blobClient = await ArmoniKClient.GetBlobService();

    await foreach (var chunk in blobContent.WithCancellation(cancellationToken))
    {
      await blobClient.UploadBlobAsync(BlobInfo,
                                       chunk,
                                       cancellationToken);
    }
  }

  public async Task UploadBlobData(ReadOnlyMemory<byte> blobContent,
                                   CancellationToken    cancellationToken)
  {
    var blobClient = await ArmoniKClient.GetBlobService();
    await blobClient.UploadBlobAsync(BlobInfo,
                                     blobContent,
                                     cancellationToken);
  }
}
