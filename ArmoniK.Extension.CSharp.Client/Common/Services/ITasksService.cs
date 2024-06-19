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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
  Task<IEnumerable<TaskInfos>> SubmitTasksAsync(SessionInfo           session,
                                                IEnumerable<TaskNode> taskNodes,
                                                CancellationToken     cancellationToken = default);

  Task<TaskState> GetTasksDetailedAsync(string            taskId,
                                        CancellationToken cancellationToken = default);

  Task<IEnumerable<TaskState>> ListTasksDetailedAsync(SessionInfo       session,
                                                      TaskPagination    paginationOptions,
                                                      CancellationToken cancellationToken = default);
}
