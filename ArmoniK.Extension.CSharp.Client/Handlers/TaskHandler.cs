using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class TaskHandler : TaskInfos
{
    public readonly ArmoniKClient ArmoniKClient;

    public TaskHandler(ArmoniKClient armoniKClient, TaskInfos taskInfo) :
        base(new SubmitTasksResponse.Types.TaskInfo
        {
            DataDependencies = { taskInfo.DataDependencies },
            ExpectedOutputIds = { taskInfo.ExpectedOutputs },
            PayloadId = taskInfo.PayloadId,
            TaskId = taskInfo.TaskId
        }, taskInfo.SessionId)
    {
        ArmoniKClient = armoniKClient;
    }

    public async Task<TaskState> GetTaskDetails(CancellationToken cancellationToken)
    {
        var taskClient = await ArmoniKClient.GetTasksService();
        return await taskClient.GetTasksDetailedAsync(TaskId, cancellationToken);
    }
}