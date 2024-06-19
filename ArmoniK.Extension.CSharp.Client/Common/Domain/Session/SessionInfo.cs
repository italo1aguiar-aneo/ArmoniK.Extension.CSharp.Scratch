namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

public class SessionInfo
{
  public readonly string SessionId;

  public SessionInfo(string sessionId)
    => SessionId = sessionId;
}
