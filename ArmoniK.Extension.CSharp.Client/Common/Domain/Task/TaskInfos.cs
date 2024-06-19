using System.Collections.Generic;

using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskInfos
{
  public TaskInfos(SubmitTasksResponse.Types.TaskInfo taskInfo,
                   string                             sessionId)
  {
    TaskId           = taskInfo.TaskId;
    ExpectedOutputs  = taskInfo.ExpectedOutputIds;
    DataDependencies = taskInfo.DataDependencies;
    PayloadId        = taskInfo.PayloadId;
    SessionId        = sessionId;
  }

  protected TaskInfos()
  {
  }

  public string              TaskId           { get; set; }
  public IEnumerable<string> ExpectedOutputs  { get; set; }
  public IEnumerable<string> DataDependencies { get; set; }
  public string              PayloadId        { get; set; }
  public string              SessionId        { get; set; }
}
