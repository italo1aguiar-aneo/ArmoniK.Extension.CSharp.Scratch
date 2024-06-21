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
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.DLLClient.Common;

namespace ArmoniK.Extension.CSharp.DLLClient;

public static class DynamicLibraryExt
{
  public static async Task<ISessionService> GetSessionService(this ArmoniKClient          armoniKClient,
                                                              IEnumerable<DynamicLibrary> dynamicLibraries)
    => await armoniKClient.GetSessionService(new DllTasksOptions(dynamicLibraries,
                                                                 armoniKClient.TaskConfiguration));

  public static async Task<DllBlob> SendDllBlob(this IBlobService    blobService,
                                                SessionInfo          session,
                                                DynamicLibrary       dynamicLibrary,
                                                ReadOnlyMemory<byte> content,
                                                CancellationToken    cancellationToken)
  {
    var blobInfo = await blobService.CreateBlobAsync(session,
                                                     dynamicLibrary.ToString(),
                                                     content,
                                                     cancellationToken);
    return new DllBlob(dynamicLibrary)
           {
             BlobId    = blobInfo.BlobId,
             SessionId = session.SessionId,
           };
  }

  public static async Task SubmitTasksWithDll(this ITasksService    taskService,
                                              SessionInfo           session,
                                              IEnumerable<TaskNode> taskNodes,
                                              DllBlob               dllBlob,
                                              CancellationToken     cancellationToken)
  {
    taskNodes = taskNodes.Select(x =>
                                 {
                                   x.DataDependencies.Add(dllBlob);
                                   if (x.TaskOptions?.Options is not null && x.TaskOptions.Options.ContainsKey(dllBlob.BlobName))
                                   {
                                     x.TaskOptions.Options.Remove(dllBlob.BlobName);
                                   }

                                   return x;
                                 });


    await taskService.SubmitTasksAsync(session,
                                       taskNodes,
                                       cancellationToken);
  }
}
