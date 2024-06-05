using System;
using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;

public class BlobState : BlobInfo
{
    public DateTime CompletedAt;
    public DateTime CreateAt;
    public ResultStatus Status;

    public BlobState()
    {
    }

    public BlobState(string blobName, string blobId, string sessionId, DateTime createAt, DateTime completedAt,
        ResultStatus status) : base(blobName, blobId, sessionId)
    {
        CreateAt = createAt;
        CompletedAt = completedAt;
        Status = status;
    }
}