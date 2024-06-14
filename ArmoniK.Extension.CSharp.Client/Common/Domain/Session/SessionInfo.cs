namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

public record SessionInfo
{
    public readonly string SessionId;

    public SessionInfo(string sessionId)
    {
        SessionId = sessionId;
    }
}