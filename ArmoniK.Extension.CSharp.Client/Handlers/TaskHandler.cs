using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class TaskHandler
{
    public readonly ArmoniKClient ArmoniKClient;
    private readonly TaskInfos TaskInfos;

    public TaskHandler(ArmoniKClient armoniKClient, TaskInfos taskInfo)
    {
        ArmoniKClient = armoniKClient;
        TaskInfos = taskInfo;
    }

    public async Task<TaskState> GetTaskDetails(CancellationToken cancellationToken)
    {
        var taskClient = await ArmoniKClient.GetTasksService();
        return await taskClient.GetTasksDetailedAsync(TaskInfos.TaskId, cancellationToken);
    }
}