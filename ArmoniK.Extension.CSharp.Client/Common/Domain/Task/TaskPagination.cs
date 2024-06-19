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

using ArmoniK.Api.gRPC.V1.SortDirection;
using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskPagination
{
  public int           Page          { get; set; }
  public int           PageSize      { get; set; }
  public int           Total         { get; set; }
  public SortDirection SortDirection { get; set; }
  public Filters       Filter        { get; set; }
}

/// <summary>
///   Represents a page within a paginated list of tasks, providing basic task status information.
/// </summary>
public record TaskPage
{
  /// <summary>
  ///   Total number of pages available in the paginated list.
  /// </summary>
  public int TotalPages { get; init; }

  /// <summary>
  ///   Unique identifier of the task.
  /// </summary>
  public string TaskId { get; init; }

  /// <summary>
  ///   Current status of the task.
  /// </summary>
  public TaskStatus TaskStatus { get; init; }
}

/// <summary>
///   Represents a detailed page within a paginated list of tasks, containing extensive information about a specific
///   task.
/// </summary>
public record TaskDetailedPage
{
  /// <summary>
  ///   Total number of pages available in the paginated list.
  /// </summary>
  public int TotalPages { get; init; }

  /// <summary>
  ///   Detailed state information of the task.
  /// </summary>
  public TaskState TaskDetails { get; init; }
}
