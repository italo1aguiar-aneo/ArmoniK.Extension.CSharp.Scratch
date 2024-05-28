using System;
using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

public class BlobState : BlobInfo
{
    public DateTime CompletedAt;
    public DateTime CreateAt;
    public ResultStatus Status;

    public BlobState()
    {
    }

    public BlobState(string name, string id, string sessionId, DateTime createAt, DateTime completedAt,
        ResultStatus status) : base(name, id, sessionId)
    {
        CreateAt = createAt;
        CompletedAt = completedAt;
        Status = status;
    }
}