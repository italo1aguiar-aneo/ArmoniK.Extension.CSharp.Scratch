using System.Collections.Generic;
using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

/// <summary>
///     Represents task characteristics that are typically derived from a task.
/// </summary>
public record TaskInfos
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TaskInfos" /> class with detailed task information from a task and
    ///     session ID.
    /// </summary>
    /// <param name="taskInfo">The task information from the task submission response.</param>
    /// <param name="sessionId">The session ID associated with the task.</param>
    internal TaskInfos(SubmitTasksResponse.Types.TaskInfo taskInfo, string sessionId)
    {
        TaskId = taskInfo.TaskId;
        ExpectedOutputs = taskInfo.ExpectedOutputIds;
        DataDependencies = taskInfo.DataDependencies;
        PayloadId = taskInfo.PayloadId;
        SessionId = sessionId;
    }

    /// <summary>
    ///     Protected constructor used for inheritance purposes, allowing derived classes to initialize without parameters.
    /// </summary>
    protected TaskInfos()
    {
    }

    /// <summary>
    ///     Identifier of the task.
    /// </summary>
    public string TaskId { get; init; }

    /// <summary>
    ///     Collection of expected output IDs.
    /// </summary>
    public IEnumerable<string> ExpectedOutputs { get; init; }

    /// <summary>
    ///     Collection of data dependencies IDs.
    /// </summary>
    public IEnumerable<string> DataDependencies { get; init; }

    /// <summary>
    ///     Identifier for the payload associated with the task.
    /// </summary>
    public string PayloadId { get; init; }

    /// <summary>
    ///     Session ID associated with the task.
    /// </summary>
    public string SessionId { get; init; }
}