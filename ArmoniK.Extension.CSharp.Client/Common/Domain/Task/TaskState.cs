using System;
using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public record TaskState : TaskInfos
{
    public DateTime? CreateAt;
    public DateTime? EndedAt;
    public DateTime? StartedAt;
    public TaskStatus? Status;

    public TaskState()
    {
    }

    public TaskState(DateTime createAt, DateTime endedAt,
        DateTime startedAt, TaskStatus status)
    {
        CreateAt = createAt;
        EndedAt = endedAt;
        StartedAt = startedAt;
        Status = status;
    }
}

//
// Résumé :
//     * Task status.
public enum TaskStatus
{
    //
    // Résumé :
    //     * Task is in an unknown state.
    [OriginalName("TASK_STATUS_UNSPECIFIED")]
    Unspecified,

    //
    // Résumé :
    //     * Task is being created in database.
    [OriginalName("TASK_STATUS_CREATING")] Creating,

    //
    // Résumé :
    //     * Task is submitted to the queue.
    [OriginalName("TASK_STATUS_SUBMITTED")]
    Submitted,

    //
    // Résumé :
    //     * Task is dispatched to a worker.
    [OriginalName("TASK_STATUS_DISPATCHED")]
    Dispatched,

    //
    // Résumé :
    //     * Task is completed.
    [OriginalName("TASK_STATUS_COMPLETED")]
    Completed,

    //
    // Résumé :
    //     * Task is in error state.
    [OriginalName("TASK_STATUS_ERROR")] Error,

    //
    // Résumé :
    //     * Task is in timeout state.
    [OriginalName("TASK_STATUS_TIMEOUT")] Timeout,

    //
    // Résumé :
    //     * Task is being cancelled.
    [OriginalName("TASK_STATUS_CANCELLING")]
    Cancelling,

    //
    // Résumé :
    //     * Task is cancelled.
    [OriginalName("TASK_STATUS_CANCELLED")]
    Cancelled,

    //
    // Résumé :
    //     * Task is being processed.
    [OriginalName("TASK_STATUS_PROCESSING")]
    Processing,

    //
    // Résumé :
    //     * Task is processed.
    [OriginalName("TASK_STATUS_PROCESSED")]
    Processed,

    //
    // Résumé :
    //     * Task is retried.
    [OriginalName("TASK_STATUS_RETRIED")] Retried
}