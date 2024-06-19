using System;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskState : TaskInfos
{
  public DateTime   CreateAt;
  public DateTime   EndedAt;
  public DateTime   StartedAt;
  public TaskStatus Status;

  public TaskState()
  {
  }

  public TaskState(SubmitTasksResponse.Types.TaskInfo taskInfo,
                   string                             sessionId,
                   DateTime                           createAt,
                   DateTime                           endedAt,
                   DateTime                           startedAt,
                   TaskStatus                         status)
    : base(taskInfo,
           sessionId)
  {
    CreateAt  = createAt;
    EndedAt   = endedAt;
    StartedAt = startedAt;
    Status    = status;
  }
}
