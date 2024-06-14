using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
    Task<IEnumerable<TaskInfos>> SubmitTasksAsync(SessionInfo session, IEnumerable<TaskNode> taskNodes,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<TaskPage> ListTasksAsync(TaskPagination paginationOptions,
        CancellationToken cancellationToken = default);

    Task<TaskState> GetTasksDetailedAsync(string taskId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TaskDetailedPage> ListTasksDetailedAsync(SessionInfo session, TaskPagination paginationOptions,
        CancellationToken cancellationToken = default);

    Task CancelTask(IEnumerable<string> taskIds, CancellationToken cancellationToken = default);
}

public static class ITasksServiceExt
{
    public static async IAsyncEnumerable<TaskPage> GetTasksAsync(this ITasksService taskService,
        IEnumerable<string> taskIds, int pageSize = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var taskPagination = new TaskPagination
        {
            Filter = new Filters
            {
                Or =
                {
                    taskIds.Select(id => TaskIdFilter(id))
                }
            },
            Page = 0,
            PageSize = 50,
            SortDirection = SortDirection.Asc
        };

        var total = 0;
        var firstPage = true;

        while (true)
        {
            await foreach (var taskPage in taskService.ListTasksAsync(taskPagination, cancellationToken))
            {
                if (firstPage)
                {
                    total = taskPage.TotalPages;
                    firstPage = false;
                }

                yield return taskPage;
            }

            taskPagination.Page++;
            if (taskPagination.Page * pageSize >= total) break;
        }
    }

    private static FiltersAnd TaskIdFilter(string taskId)
    {
        return new FiltersAnd
        {
            And =
            {
                new FilterField
                {
                    Field = new TaskField
                    {
                        TaskSummaryField = new TaskSummaryField
                        {
                            Field = TaskSummaryEnumField.TaskId
                        }
                    },
                    FilterString = new FilterString
                    {
                        Value = taskId,
                        Operator = FilterStringOperator.Equal
                    }
                }
            }
        };
    }
}