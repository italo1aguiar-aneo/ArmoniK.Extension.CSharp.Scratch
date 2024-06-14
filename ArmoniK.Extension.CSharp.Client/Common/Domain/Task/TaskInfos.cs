using System.Collections.Generic;
using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public record TaskInfos
{
    internal TaskInfos(SubmitTasksResponse.Types.TaskInfo taskInfo, string sessionId)
    {
        TaskId = taskInfo.TaskId;
        ExpectedOutputs = taskInfo.ExpectedOutputIds;
        DataDependencies = taskInfo.DataDependencies;
        PayloadId = taskInfo.PayloadId;
        SessionId = sessionId;
    }

    protected TaskInfos()
    {
    }

    public string TaskId { get; init; }
    public IEnumerable<string> ExpectedOutputs { get; init; }
    public IEnumerable<string> DataDependencies { get; init; }
    public string PayloadId { get; init; }
    public string SessionId { get; init; }
}