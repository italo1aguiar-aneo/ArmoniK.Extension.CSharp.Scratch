namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

/// <summary>
///     Represents session information, specifically encapsulating a session identifier.
/// </summary>
public record SessionInfo
{
    /// <summary>
    ///     Identifier of the session.
    /// </summary>
    public readonly string SessionId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionInfo" /> record with a specified session identifier.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    public SessionInfo(string sessionId)
    {
        SessionId = sessionId;
    }
}