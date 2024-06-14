using System;
using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

/// <summary>
///     Represents the state of a task at any given moment, extending the TaskInfos.
/// </summary>
public record TaskState : TaskInfos
{
    /// <summary>
    ///     Initializes a new instance of the TaskState class.
    /// </summary>
    public TaskState()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the TaskState class with details about task timings and status.
    /// </summary>
    /// <param name="createAt">The creation time of the task.</param>
    /// <param name="endedAt">The end time of the task.</param>
    /// <param name="startedAt">The start time of the task.</param>
    /// <param name="status">The status of the task.</param>
    public TaskState(DateTime createAt, DateTime endedAt,
        DateTime startedAt, TaskStatus status)
    {
        CreateAt = createAt;
        EndedAt = endedAt;
        StartedAt = startedAt;
        Status = status;
    }

    /// <summary>
    ///     Time when the task was created.
    /// </summary>
    public DateTime? CreateAt { get; init; }

    /// <summary>
    ///     Time when the task ended.
    /// </summary>
    public DateTime? EndedAt { get; init; }

    /// <summary>
    ///     Time when the task started.
    /// </summary>
    public DateTime? StartedAt { get; init; }

    /// <summary>
    ///     Current status of the task.
    /// </summary>
    public TaskStatus? Status { get; init; }
}

/// <summary>
///     Defines the various statuses a task can have through its lifecycle.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    ///     The task is in an unknown state.
    /// </summary>
    [OriginalName("TASK_STATUS_UNSPECIFIED")]
    Unspecified,

    /// <summary>
    ///     The task is being created in the database.
    /// </summary>
    [OriginalName("TASK_STATUS_CREATING")] Creating,

    /// <summary>
    ///     The task has been submitted to the queue.
    /// </summary>
    [OriginalName("TASK_STATUS_SUBMITTED")]
    Submitted,

    /// <summary>
    ///     The task is dispatched to a worker.
    /// </summary>
    [OriginalName("TASK_STATUS_DISPATCHED")]
    Dispatched,

    /// <summary>
    ///     The task is completed.
    /// </summary>
    [OriginalName("TASK_STATUS_COMPLETED")]
    Completed,

    /// <summary>
    ///     The task is in an error state.
    /// </summary>
    [OriginalName("TASK_STATUS_ERROR")] Error,

    /// <summary>
    ///     The task is in timeout state.
    /// </summary>
    [OriginalName("TASK_STATUS_TIMEOUT")] Timeout,

    /// <summary>
    ///     The task is being cancelled.
    /// </summary>
    [OriginalName("TASK_STATUS_CANCELLING")]
    Cancelling,

    /// <summary>
    ///     The task is cancelled.
    /// </summary>
    [OriginalName("TASK_STATUS_CANCELLED")]
    Cancelled,

    /// <summary>
    ///     The task is being processed.
    /// </summary>
    [OriginalName("TASK_STATUS_PROCESSING")]
    Processing,

    /// <summary>
    ///     The task is processed.
    /// </summary>
    [OriginalName("TASK_STATUS_PROCESSED")]
    Processed,

    /// <summary>
    ///     The task is being retried.
    /// </summary>
    [OriginalName("TASK_STATUS_RETRIED")] Retried
}