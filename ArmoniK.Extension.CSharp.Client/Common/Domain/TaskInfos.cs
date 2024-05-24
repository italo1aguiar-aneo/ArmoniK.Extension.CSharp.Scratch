using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Api.gRPC.V1.Tasks;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain
{
    public class TaskInfos
    {
        public string TaskId { get; set; }
        public IEnumerable<string> ExpectedOutputs { get; set; }
        public IEnumerable<string> DataDependencies { get; set; }
        public string PayloadId { get; set; }

        public TaskInfos(SubmitTasksResponse.Types.TaskInfo taskInfo)
        {
            TaskId = taskInfo.TaskId;
            ExpectedOutputs = taskInfo.ExpectedOutputIds;
            DataDependencies = taskInfo.DataDependencies;
            PayloadId = taskInfo.PayloadId;
        }
    }
}
