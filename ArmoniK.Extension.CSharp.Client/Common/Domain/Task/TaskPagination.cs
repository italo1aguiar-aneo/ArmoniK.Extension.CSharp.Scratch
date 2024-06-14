using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.Extension.CSharp.Client.Common.Enum;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

/// <summary>
///     Represents pagination details for tasks, allowing for sorted and filtered lists of tasks.
/// </summary>
public class TaskPagination
{
    /// <summary>
    ///     page number of the result set.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    ///     Number of tasks per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Total number of tasks available.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    ///     Direction in which the task results are sorted.
    /// </summary>
    public SortDirection SortDirection { get; set; }

    /// <summary>
    ///     Filters applied to the task query.
    /// </summary>
    public Filters Filter { get; set; }
}

/// <summary>
///     Represents a page within a paginated list of tasks, providing basic task status information.
/// </summary>
public record TaskPage
{
    /// <summary>
    ///     Total number of pages available in the paginated list.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    ///     Unique identifier of the task.
    /// </summary>
    public string TaskId { get; init; }

    /// <summary>
    ///     Current status of the task.
    /// </summary>
    public TaskStatus TaskStatus { get; init; }
}

/// <summary>
///     Represents a detailed page within a paginated list of tasks, containing extensive information about a specific
///     task.
/// </summary>
public record TaskDetailedPage
{
    /// <summary>
    ///     Total number of pages available in the paginated list.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    ///     Detailed state information of the task.
    /// </summary>
    public TaskState TaskDetails { get; init; }
}