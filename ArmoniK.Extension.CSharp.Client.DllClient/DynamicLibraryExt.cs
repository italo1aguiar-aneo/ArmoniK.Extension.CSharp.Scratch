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

using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.DllClient.Common;

namespace ArmoniK.Extension.CSharp.Client.DllClient;

/// <summary>
///   Provides extension methods for handling dynamic library usage on ArmoniK's environment.
///   These methods facilitate session management, dynamic library uploading, and task submissions using dynamic libraries.
/// </summary>
public static class DynamicLibraryExt
{
  /// <summary>
  ///   Retrieves an <see cref="ISessionService" /> instance for managing sessions with the specified dynamic libraries.
  /// </summary>
  /// <param name="armoniKClient">The ArmoniK client used to access session services.</param>
  /// <param name="dynamicLibraries">A collection of dynamic libraries that the session will handle.</param>
  /// <returns>
  ///   A task that represents the asynchronous operation. The task result contains the <see cref="ISessionService" />
  ///   instance.
  /// </returns>
  public static async Task<ISessionService> GetSessionService(this ArmoniKClient          armoniKClient,
                                                              IEnumerable<DynamicLibrary> dynamicLibraries)
    => await armoniKClient.GetSessionService(new DllTasksConfiguration(dynamicLibraries,
                                                                       armoniKClient.TaskConfiguration));

  /// <summary>
  ///   Asynchronously sends a dynamic library blob to a blob service
  /// </summary>
  /// <param name="blobService">The blob service to use for uploading the library.</param>
  /// <param name="session">The session information associated with the blob upload.</param>
  /// <param name="dynamicLibrary">The dynamic library related to the blob being sent.</param>
  /// <param name="content">The binary content of the dynamic library to upload.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>
  ///   The created <see cref="DllBlob" /> instance with relevant identifiers.
  /// </returns>
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

  /// <summary>
  ///   Submits tasks with task service, depending on a previously uploaded dynamic library blob.
  /// </summary>
  /// <param name="taskService">The service responsible for handling task submissions.</param>
  /// <param name="session">Session which must contain the tasks.</param>
  /// <param name="taskNodes">The collection of tasks to submit.</param>
  /// <param name="dllBlob">The dynamic library blob dependency for the tasks.</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests during the task submission process.</param>
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