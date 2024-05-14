using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
    Task<IEnumerable<TaskInfos>> SubmitTasksAsync(SessionInfo session, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default);

    Task<TaskState> GetTasksDetailedAsync(string taskId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskState>> ListTasksDetailedAsync(SessionInfo session, TaskPagination paginationOptions,
        CancellationToken cancellationToken = default);
}