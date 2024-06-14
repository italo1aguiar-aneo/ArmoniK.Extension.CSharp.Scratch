using Google.Protobuf.Reflection;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Health;

public record Health
{
    public string Name { get; init; }
    public string Message { get; init; }
    public HealthStatusEnum Status { get; init; }
}

public enum HealthStatusEnum
{
    /// <summary>* Unspecified</summary>
    [OriginalName("HEALTH_STATUS_ENUM_UNSPECIFIED")]
    Unspecified,

    /// <summary>* Service is working without issues</summary>
    [OriginalName("HEALTH_STATUS_ENUM_HEALTHY")]
    Healthy,

    /// <summary>* Service has issues but still works</summary>
    [OriginalName("HEALTH_STATUS_ENUM_DEGRADED")]
    Degraded,

    /// <summary>* Service does not work</summary>
    [OriginalName("HEALTH_STATUS_ENUM_UNHEALTHY")]
    Unhealthy
}