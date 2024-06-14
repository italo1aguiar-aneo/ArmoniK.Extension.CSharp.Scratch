using System;
using System.Collections.Generic;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public record TaskConfiguration
{
    public TaskConfiguration(int maxRetries, int priority, string partitionId,
        TimeSpan maxDuration, Dictionary<string, string> options = null,
        ApplicationConfiguration applicationConfiguration = null)
    {
        MaxRetries = maxRetries;
        Priority = priority;
        PartitionId = partitionId;
        Options = options;
        MaxDuration = maxDuration;
        ApplicationConfiguration = applicationConfiguration;
    }

    public int MaxRetries { get; init; }
    public int Priority { get; init; }
    public string PartitionId { get; init; }
    [CanBeNull] public Dictionary<string, string> Options { get; init; }
    public TimeSpan MaxDuration { get; init; }
    [CanBeNull] public ApplicationConfiguration ApplicationConfiguration { get; init; }

    public TaskOptions ToTaskOptions()
    {
        var taskOptions = new TaskOptions
        {
            MaxRetries = MaxRetries,
            Priority = Priority,
            PartitionId = PartitionId,
            MaxDuration = Duration.FromTimeSpan(MaxDuration)
        };

        if (Options != null) taskOptions.Options.Add(Options);

        if (ApplicationConfiguration != null)
        {
            taskOptions.ApplicationName = ApplicationConfiguration?.ApplicationName;
            taskOptions.ApplicationService = ApplicationConfiguration?.ApplicationService;
            taskOptions.ApplicationNamespace = ApplicationConfiguration?.ApplicationNamespace;
            taskOptions.ApplicationVersion = ApplicationConfiguration?.ApplicationVersion;
        }

        return taskOptions;
    }
}