using System.Collections.Generic;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;

/// <summary>
///     Represents a partition within a ArmoniK, detailing its configuration, resource allocation, and
///     hierarchy.
/// </summary>
public record Partition
{
    /// <summary>
    ///     Identifier of the partition.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    ///     Collection of identifiers for parent partitions.
    /// </summary>
    public IEnumerable<string> ParentPartitionIds { get; init; }

    /// <summary>
    ///     Configuration settings for pods within the partition, represented as key-value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> PodConfiguration { get; init; }

    /// <summary>
    ///     Maximum number of pods that can be allocated to this partition.
    /// </summary>
    public long PodMax { get; init; }

    /// <summary>
    ///     Number of pods that are reserved for this partition.
    /// </summary>
    public long PodReserved { get; init; }

    /// <summary>
    ///     Percentage of the partition's capacity that is subject to preemption.
    /// </summary>
    public long PreemptionPercentage { get; init; }

    /// <summary>
    ///     Priority of the partition, which may influence scheduling decisions.
    /// </summary>
    public long Priority { get; init; }
}