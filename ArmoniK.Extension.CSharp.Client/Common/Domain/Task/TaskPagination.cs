using ArmoniK.Api.gRPC.V1.Tasks;
using SortDirection = ArmoniK.Extension.CSharp.Client.Common.Enum.SortDirection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskPagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public SortDirection SortDirection { get; set; }
    public Filters Filter { get; set; }
}

public record TaskPage
{
    public int TotalPages { get; init; }
    public string TaskId { get; init; }
    public TaskStatus TaskStatus { get; init; }
}

public record TaskDetailedPage
{
    public int TotalPages { get; init; }
    public TaskState TaskDetails { get; init; }
}