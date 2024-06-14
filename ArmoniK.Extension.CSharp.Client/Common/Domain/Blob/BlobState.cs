using System;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public record BlobState : BlobInfo
{
    public DateTime? CompletedAt;
    public DateTime? CreateAt;
    public BlobStatus? Status;
}

public enum BlobStatus
{
    /// <summary>* Blob is in an unspecified state.</summary>
    Unspecified = 0,

    /// <summary>
    ///     * Blob is created and task is created, submitted or dispatched.
    /// </summary>
    Created = 1,

    /// <summary>* Blob is completed with a completed task.</summary>
    Completed = 2,

    /// <summary>* Blob is aborted.</summary>
    Aborted = 3,

    /// <summary>
    ///     * Blob is completed, but data has been deleted from object storage.
    /// </summary>
    Deleted = 4,

    /// <summary>
    ///     * NOTFOUND is encoded as 127 to make it small while still leaving enough room for future status extensions
    ///     see https://developers.google.com/protocol-buffers/docs/proto3#enum
    /// </summary>
    Notfound = 127 // 0x0000007F
}