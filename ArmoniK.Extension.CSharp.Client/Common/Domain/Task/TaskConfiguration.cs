using System;
using System.Collections.Generic;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Task;

/// <summary>
///     Configuration settings for a task, including retry policies, priorities, partitioning details, and
///     application-specific configurations.
/// </summary>
public record TaskConfiguration
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TaskConfiguration" /> class with specified task configuration
    ///     settings.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries for the task.</param>
    /// <param name="priority">The priority level of the task.</param>
    /// <param name="partitionId">The partition identifier for task segregation.</param>
    /// <param name="maxDuration">The maximum duration allowed for the task to complete.</param>
    /// <param name="options">Optional additional key-value pairs for further customization.</param>
    /// <param name="applicationConfiguration">Optional application-specific configurations.</param>
    public TaskConfiguration(int maxRetries, int priority, string partitionId,
        TimeSpan maxDuration, Dictionary<string, string> options = null,
        ApplicationConfiguration applicationConfiguration = null)
    {
        MaxRetries = maxRetries;
        Priority = priority;
        PartitionId = partitionId;
        Options = options ?? new Dictionary<string, string>(); // Ensure options is never null
        MaxDuration = maxDuration;
        ApplicationConfiguration = applicationConfiguration;
    }

    /// <summary>
    ///     Maximum number of retries for the task.
    /// </summary>
    public int MaxRetries { get; init; }

    /// <summary>
    ///     Priority level of the task.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    ///    Partition identifier used for task segregation.
    /// </summary>
    public string PartitionId { get; init; }

    /// <summary>
    ///     Key-value pair options for task configuration.
    /// </summary>
    [CanBeNull]
    public Dictionary<string, string> Options { get; init; }

    /// <summary>
    ///     Maximum duration allowed for the task to run.
    /// </summary>
    public TimeSpan MaxDuration { get; init; }

    /// <summary>
    ///     Optional application-specific configuration.
    /// </summary>
    [CanBeNull]
    public ApplicationConfiguration ApplicationConfiguration { get; init; }

    /// <summary>
    ///     Converts this <see cref="TaskConfiguration" /> instance to a <see cref="TaskOptions" /> suitable for use with task
    ///     submission.
    /// </summary>
    /// <returns>A new <see cref="TaskOptions" /> instance populated with the settings from this configuration.</returns>
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

        if (ApplicationConfiguration == null) return taskOptions;

        taskOptions.ApplicationName = ApplicationConfiguration.ApplicationName;
        taskOptions.ApplicationService = ApplicationConfiguration.ApplicationService;
        taskOptions.ApplicationNamespace = ApplicationConfiguration.ApplicationNamespace;
        taskOptions.ApplicationVersion = ApplicationConfiguration.ApplicationVersion;

        return taskOptions;
    }
}