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

using System.Runtime.Loader;

using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Extension.CSharp.DllCommon;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Worker;

public class LibraryWorker : ILibraryWorker
{
  public LibraryWorker(IConfiguration configuration,
                       ILoggerFactory factory)
  {
    Configuration = configuration;
    LoggerFactory = factory;
    Logger        = factory.CreateLogger<LibraryWorker>();
  }

  private ILogger<LibraryWorker> Logger { get; }

  public ILoggerFactory LoggerFactory { get; set; }

  public IConfiguration Configuration { get; set; }

  public async Task<Output> Execute(ITaskHandler      taskHandler,
                                    ILibraryLoader    libraryLoader,
                                    string            libraryContext,
                                    CancellationToken cancellationToken)
  {
    using var _ = Logger.BeginPropertyScope(("sessionId", taskHandler.SessionId),
                                            ("taskId", $"{taskHandler.TaskId}"));

    var serviceLibrary = taskHandler.TaskOptions.GetServiceLibrary();

    var dynamicLibrary = DynamicLibraryExt.GetDynamicLibraryFromTaskOptions(taskHandler.TaskOptions,
                                                                            serviceLibrary);

    Logger.LogInformation($"ServiceLibrary: {serviceLibrary}");
    Logger.LogInformation($"DynamicLibrary.Service: {dynamicLibrary.Service}");

    if (dynamicLibrary.Service == null || serviceLibrary == null)
    {
      throw new WorkerApiException("No ServiceLibrary found");
    }

    Logger.LogInformation("Entering Context");

    var context = libraryLoader.GetAssemblyLoadContext(libraryContext);
    try
    {
      //Logger.LogInformation($"Before loading: assembly context {AssemblyLoadContext.CurrentContextualReflectionContext?.Name}");
      if (AssemblyLoadContext.CurrentContextualReflectionContext == null || AssemblyLoadContext.CurrentContextualReflectionContext?.Name != context.Name)
      {
        context.EnterContextualReflection();
      }

      //Logger.LogInformation($"After loading: Current assembly context {AssemblyLoadContext.CurrentContextualReflectionContext?.Name}");
      var serviceClass = libraryLoader.GetClassInstance<object>(dynamicLibrary);
      if (serviceClass == null)
      {
        throw new WorkerApiException("ServiceClass does not exists");
      }

      if (serviceClass.GetType()
                      .GetInterfaces()
                      .All(x => x != typeof(IDllWorker)))
      {
        throw new WorkerApiException($"ServiceClass must implement {typeof(IDllWorker)}");
      }

      var executionMethod = serviceClass.GetType()
                                        .GetMethod("Execute");

      var task = (Task<Output>)executionMethod.Invoke(serviceClass,
                                                      new object?[]
                                                      {
                                                        taskHandler,
                                                        Logger,
                                                        cancellationToken,
                                                      });

      var result = await task.ConfigureAwait(false);
      Logger.LogInformation($"Got the following result from the execution:{result}");

      return result;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      return new Output
             {
               Error = new Output.Types.Error
                       {
                         Details = ex.Message,
                       },
             };
    }
  }
}
