using System;
using System.Collections.Generic;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

public class TaskConfiguration
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

    public int MaxRetries { get; set; }
    public int Priority { get; set; }
    public string PartitionId { get; set; }
    [CanBeNull] public Dictionary<string, string> Options { get; set; }
    public TimeSpan MaxDuration { get; set; }
    [CanBeNull] public ApplicationConfiguration ApplicationConfiguration { get; set; }

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