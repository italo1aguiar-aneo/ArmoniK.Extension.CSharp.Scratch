using System.Collections.Generic;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Partition;

public record Partition
{
    public string Id { get; init; }
    public IEnumerable<string> ParentPartitionIds { get; init; }
    public IEnumerable<KeyValuePair<string, string>> PodConfiguration { get; init; }
    public long PodMax { get; init; }
    public long PodReserved { get; init; }
    public long PreemptionPercentage { get; init; }
    public long Priority { get; init; }
}