using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain
{
    public class TaskConfiguration
    {
        public TaskConfiguration(int maxRetries, int priority, string partitionId,
            TimeSpan maxDuration, Dictionary<string, string> options = null, ApplicationConfiguration applicationConfiguration = null)
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
                MaxRetries = this.MaxRetries,
                Priority = this.Priority,
                PartitionId = this.PartitionId,
                MaxDuration = Duration.FromTimeSpan(this.MaxDuration),
            };

            if (this.Options != null)
            {
                taskOptions.Options.Add(this.Options);
            }

            if (this.ApplicationConfiguration != null)
            {
                taskOptions.ApplicationName = this.ApplicationConfiguration?.ApplicationName;
                taskOptions.ApplicationService = this.ApplicationConfiguration?.ApplicationService;
                taskOptions.ApplicationNamespace = this.ApplicationConfiguration?.ApplicationNamespace;
                taskOptions.ApplicationVersion = this.ApplicationConfiguration?.ApplicationVersion;
            }

            return taskOptions;
        }
    }
}
