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

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.DllWorker.Exceptions;

using JetBrains.Annotations;

namespace ArmoniK.Extension.CSharp.DllWorker;

public class ArmonikServiceWorker : IDisposable
{
  public List<DynamicLibrary> DynamicLibraries = new();

  public void Dispose()
  {
  }

  public bool Initialized { get; set; }

  public void Configure(IConfiguration configuration,
                        TaskOptions    requestTaskOptions)
  {
    if (Initialized)
    {
      return;
    }

    using (AppsLoader.UserAssemblyLoadContext.EnterContextualReflection())
    {
      GridWorker.Configure(configuration,
                           requestTaskOptions,
                           AppsLoader);
    }

    Initialized = true;
  }
}

public class ServiceRequestContext
{
  private readonly ILogger<ServiceRequestContext> logger_;

  [CanBeNull]
  private ArmonikServiceWorker currentService_;

  public ServiceRequestContext(ILoggerFactory loggerFactory)
  {
    LoggerFactory   = loggerFactory;
    currentService_ = null;
    logger_         = loggerFactory.CreateLogger<ServiceRequestContext>();
  }

  public Session SessionId { get; set; }

  public ILoggerFactory LoggerFactory { get; set; }

  public bool IsNewSessionId(Session sessionId)
  {
    if (SessionId == null)
    {
      return true;
    }

    return SessionId.Id != sessionId.Id;
  }

  public bool IsNewSessionId(string sessionId)
  {
    if (sessionId == null)
    {
      throw new ArgumentNullException(nameof(sessionId));
    }

    if (SessionId == null)
    {
      return true;
    }

    var currentSessionId = new Session
                           {
                             Id = sessionId,
                           };

    return IsNewSessionId(currentSessionId);
  }

  public ArmonikServiceWorker CreateOrGetArmonikService(IConfiguration configuration,
                                                        string         engineTypeName,
                                                        TaskOptions    requestTaskOptions)
  {
    if (requestTaskOptions.Options.Any(x => x.Key.Contains("dll")))
    {
      throw new WorkerApiException("Cannot find dll services defined in TaskOptions.");
    }

    var serviceId = new ServiceId(packageId,
                                  requestTaskOptions.ApplicationNamespace,
                                  EngineTypeHelper.ToEnum(engineTypeName));

    if (currentService_?.ServiceId == serviceId)
    {
      return currentService_;
    }

    logger_.LogInformation($"Worker needs to load new context, from {currentService_?.ServiceId.ToString() ?? "null"} to {serviceId}");

    currentService_?.DestroyService();
    currentService_?.Dispose();
    currentService_ = null;


    var appsLoader = new AppsLoader(appPackageManager,
                                    LoggerFactory,
                                    engineTypeName,
                                    packageId);

    currentService_ = new ArmonikServiceWorker
                      {
                        AppsLoader = appsLoader,
                        GridWorker = appsLoader.GetGridWorkerInstance(configuration,
                                                                      LoggerFactory),
                        ServiceId = serviceId,
                      };

    currentService_.Configure(configuration,
                              requestTaskOptions);

    return currentService_;
  }
}
