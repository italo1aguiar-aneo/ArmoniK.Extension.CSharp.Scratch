using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

/// <summary>
///     Handles operations related to tasks using the ArmoniK client.
/// </summary>
public class TaskHandler
{
    /// <summary>
    ///     Gets the ArmoniK client used to interact with task services.
    /// </summary>
    public readonly ArmoniKClient ArmoniKClient;

    /// <summary>
    ///     Gets the task information for which this handler will perform operations.
    /// </summary>
    private readonly TaskInfos TaskInfos;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TaskHandler" /> class with a specified ArmoniK client and task
    ///     information.
    /// </summary>
    /// <param name="armoniKClient">The ArmoniK client to be used for task service operations.</param>
    /// <param name="taskInfo">The task information related to the tasks that will be handled.</param>
    public TaskHandler(ArmoniKClient armoniKClient, TaskInfos taskInfo)
    {
        ArmoniKClient = armoniKClient;
        TaskInfos = taskInfo;
    }

    /// <summary>
    ///     Asynchronously retrieves detailed state information about the task associated with this handler.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
    /// <returns>
    ///     A <see cref="Task{TaskState}" /> representing the asynchronous operation, with the task's detailed state as
    ///     the result.
    /// </returns>
    public async Task<TaskState> GetTaskDetails(CancellationToken cancellationToken)
    {
        var taskClient = await ArmoniKClient.GetTasksService();
        return await taskClient.GetTasksDetailedAsync(TaskInfos.TaskId, cancellationToken);
    }
}