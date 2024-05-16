using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using Google.Protobuf;

namespace ArmoniK.Extension.CSharp.Client.Common.Services;

public interface ITasksService
{
    Task<IEnumerable<string>> SubmitTasksAsync(IEnumerable<TaskNode> taskNodes, Session session);
}