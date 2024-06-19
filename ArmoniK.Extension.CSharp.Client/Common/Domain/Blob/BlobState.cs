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

using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public class BlobState : BlobInfo
{
  public DateTime     CompletedAt;
  public DateTime     CreateAt;
  public ResultStatus Status;

  public BlobState()
  {
  }

  public BlobState(string       blobName,
                   string       blobId,
                   string       sessionId,
                   DateTime     createAt,
                   DateTime     completedAt,
                   ResultStatus status)
    : base(blobName,
           blobId,
           sessionId)
  {
    CreateAt    = createAt;
    CompletedAt = completedAt;
    Status      = status;
  }
}
