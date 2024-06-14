namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Versions;

/// <summary>
///     Record that has the ArmoniK Version Information
/// </summary>
public record VersionsInfo
{
    /// <summary>
    ///     Version of ArmoniK Core
    /// </summary>
    public string Core { get; init; }

    /// <summary>
    ///     Version of ArmoniK Api
    /// </summary>
    public string Api { get; init; }
}