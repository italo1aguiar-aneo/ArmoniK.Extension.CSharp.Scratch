using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
    Task<IEnumerable<TaskInfos>> SubmitTasksAsync(string sessionId, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default);

    Task<TaskState> GetTasksDetailedAsync(string taskId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskState>> ListTasksDetailedAsync(string sessionId, TaskPagination paginationOptions,
        CancellationToken cancellationToken = default);
}