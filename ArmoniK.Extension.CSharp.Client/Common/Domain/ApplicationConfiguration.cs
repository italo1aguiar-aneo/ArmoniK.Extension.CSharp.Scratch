namespace ArmoniK.Extension.CSharp.Client.Common.Domain;

/// <summary>
///     Represents the configuration of an application, including its name, version, namespace, service, and engine type.
/// </summary>
public record ApplicationConfiguration
{
    /// <summary>
    ///     Name of the application.
    /// </summary>
    public string ApplicationName { get; init; }

    /// <summary>
    ///     Version of the application.
    /// </summary>
    public string ApplicationVersion { get; init; }

    /// <summary>
    ///     Namespace of the application.
    /// </summary>
    public string ApplicationNamespace { get; init; }

    /// <summary>
    ///     Service name of the application.
    /// </summary>
    public string ApplicationService { get; init; }

    /// <summary>
    ///     Type of engine used by the application.
    /// </summary>
    public string EngineType { get; init; }
}