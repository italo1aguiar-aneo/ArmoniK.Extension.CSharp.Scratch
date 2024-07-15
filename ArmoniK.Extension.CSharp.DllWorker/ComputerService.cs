// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2024. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Options;
using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.Worker;

using Grpc.Core;

namespace ArmoniK.Extension.CSharp.DllWorker;

public class ComputerService : WorkerStreamWrapper
{
  public ComputerService(ServiceRequestContext serviceRequestContext,
                         ILoggerFactory        loggerFactory,
                         ComputePlane          computePlaneOptions,
                         GrpcChannelProvider   provider)
    : base(loggerFactory,
           computePlaneOptions,
           provider)
  {
    ServiceRequestContext = serviceRequestContext;
    Logger                = loggerFactory.CreateLogger<ComputerService>();
  }

  public ServiceRequestContext    ServiceRequestContext { get; set; }
  public ILogger<ComputerService> Logger                { get; set; }

  public override async Task<Output> ProcessAsync(ITaskHandler      taskHandler,
                                                  CancellationToken cancellationToken)
  {
    using var scopedLog = Logger.BeginNamedScope("Execute task",
                                                 ("Session", taskHandler.SessionId),
                                                 ("TaskId", taskHandler.TaskId));
    Logger.LogTrace("DataDependencies {DataDependencies}",
                    taskHandler.DataDependencies.Keys);
    Logger.LogTrace("ExpectedResults {ExpectedResults}",
                    taskHandler.ExpectedResults);
    Output output;
    try
    {
      await ServiceRequestContext.ExecuteTask(taskHandler,
                                              cancellationToken);

      output = new Output
               {
                 Ok = new Empty(),
               };
    }
    catch (WorkerApiException ex)
    {
      Logger.LogError(ex,
                      "WorkerAPIException failure while executing task");

      return new Output
             {
               Error = new Output.Types.Error
                       {
                         Details = ex.Message + Environment.NewLine + ex.StackTrace,
                       },
             };
    }
    catch (RpcException ex)
    {
      Logger.LogWarning(ex,
                        "Worker sent an error");
      throw;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex,
                      "Unmanaged exception while executing task");

      throw new RpcException(new Status(StatusCode.Internal,
                                        ex.Message + Environment.NewLine + ex.StackTrace)); // Is this right ?
    }

    return output;
  }
}
