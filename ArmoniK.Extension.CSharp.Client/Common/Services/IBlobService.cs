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
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface IBlobService
{
  Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo       session,
                                         string            name,
                                         CancellationToken cancellationToken = default);

  Task<BlobInfo> CreateBlobMetadataAsync(SessionInfo       session,
                                         CancellationToken cancellationToken = default);

  Task<BlobInfo> CreateBlobFromChunksAsync(SessionInfo                            session,
                                           string                                 name,
                                           IAsyncEnumerable<ReadOnlyMemory<byte>> contents,
                                           CancellationToken                      cancellationToken = default);

  Task<BlobInfo> CreateBlobAsync(SessionInfo          session,
                                 string               name,
                                 ReadOnlyMemory<byte> content,
                                 CancellationToken    cancellationToken = default);

  Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo       session,
                                                       int               quantity,
                                                       CancellationToken cancellationToken = default);

  Task<IEnumerable<BlobInfo>> CreateBlobsMetadataAsync(SessionInfo         session,
                                                       IEnumerable<string> names,
                                                       CancellationToken   cancellationToken = default);

  Task<IEnumerable<BlobInfo>> CreateBlobsAsync(SessionInfo                                             session,
                                               IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> blobKeyValuePairs,
                                               CancellationToken                                       cancellationToken = default);

  Task<byte[]> DownloadBlobAsync(BlobInfo          blobInfo,
                                 CancellationToken cancellationToken = default);

  IAsyncEnumerable<byte[]> DownloadBlobWithChunksAsync(BlobInfo          blobInfo,
                                                       CancellationToken cancellationToken = default);

  Task UploadBlobAsync(BlobInfo             blobInfo,
                       ReadOnlyMemory<byte> blobContent,
                       CancellationToken    cancellationToken = default);

  Task UploadBlobChunkAsync(BlobInfo                               blobInfo,
                            IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent,
                            CancellationToken                      cancellationToken = default);

  Task UploadBlobsAsync(IEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                        CancellationToken                                  cancellationToken = default);

  Task UploadBlobsChunksAsync(IAsyncEnumerable<Tuple<BlobInfo, ReadOnlyMemory<byte>>> blobs,
                              CancellationToken                                       cancellationToken = default);

  Task<BlobState> GetBlobStateAsync(BlobInfo          blobInfo,
                                    CancellationToken cancellationToken = default);

  Task<IEnumerable<BlobState>> ListBlobsAsync(BlobPagination    blobPagination,
                                              CancellationToken cancellationToken = default);
}
