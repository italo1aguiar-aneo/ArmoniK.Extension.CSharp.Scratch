using System;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

/// <summary>
///     Represents the state of a blob.
/// </summary>
public record BlobState : BlobInfo
{
    /// <summary>
    ///     Datetime when the blob was set to status completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    ///     Datetime when the blob was created.
    /// </summary>
    public DateTime? CreateAt { get; init; }

    /// <summary>
    ///     Current status of the blob.
    /// </summary>
    public BlobStatus? Status { get; init; }
}

/// <summary>
///     Defines the various statuses that a blob can have.
/// </summary>
public enum BlobStatus
{
    /// <summary>
    ///     Blob is in an unspecified state.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    ///     Blob is created and the task is created, submitted, or dispatched.
    /// </summary>
    Created = 1,

    /// <summary>
    ///     Blob is completed with a completed task.
    /// </summary>
    Completed = 2,

    /// <summary>
    ///     Blob is aborted.
    /// </summary>
    Aborted = 3,

    /// <summary>
    ///     Blob was deleted.
    /// </summary>
    Deleted = 4,

    /// <summary>
    ///     NOTFOUND is encoded as 127 to make it small while still leaving enough room for future status extensions.
    ///     See https://developers.google.com/protocol-buffers/docs/proto3#enum.
    /// </summary>
    Notfound = 127 // 0x0000007F
}