using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Health;

/// <summary>
///     Represents the health status of a component.
/// </summary>
public record Health
{
    /// <summary>
    ///     Name of the component.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    ///     Message providing additional details about the health status.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    ///     Current health status of the component.
    /// </summary>
    public HealthStatusEnum Status { get; init; }
}

/// <summary>
///     Defines the health status values for a component.
/// </summary>
public enum HealthStatusEnum
{
    /// <summary>
    ///     The health status is unspecified.
    /// </summary>
    [OriginalName("HEALTH_STATUS_ENUM_UNSPECIFIED")]
    Unspecified,

    /// <summary>
    ///     The service is working without issues.
    /// </summary>
    [OriginalName("HEALTH_STATUS_ENUM_HEALTHY")]
    Healthy,

    /// <summary>
    ///     The service has issues but still works.
    /// </summary>
    [OriginalName("HEALTH_STATUS_ENUM_DEGRADED")]
    Degraded,

    /// <summary>
    ///     The service does not work.
    /// </summary>
    [OriginalName("HEALTH_STATUS_ENUM_UNHEALTHY")]
    Unhealthy
}