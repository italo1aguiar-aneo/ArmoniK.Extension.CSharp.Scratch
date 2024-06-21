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

using System;
using System.Threading.Tasks;

using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Extension.CSharp.Client.Factory;
using ArmoniK.Extension.CSharp.Client.Handlers;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client;

public class ArmoniKClient
{
  private readonly ILogger                 logger_;
  private readonly ILoggerFactory          loggerFactory_;
  private readonly Properties              properties_;
  public readonly  TaskConfiguration       TaskConfiguration;
  private          IBlobService            blobService_;
  private          ObjectPool<ChannelBase> channelPool_;
  private          IEventsService          eventsService_;
  private          ISessionService         sessionService_;
  private          ITasksService           tasksService_;

  public ArmoniKClient(Properties     properties,
                       ILoggerFactory loggerFactory)
  {
    properties_    = properties    ?? throw new ArgumentNullException(nameof(properties));
    loggerFactory_ = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    logger_        = loggerFactory.CreateLogger<ArmoniKClient>();
  }

  public ArmoniKClient(Properties        properties,
                       ILoggerFactory    loggerFactory,
                       TaskConfiguration taskConfiguration)
  {
    properties_       = properties    ?? throw new ArgumentNullException(nameof(properties));
    loggerFactory_    = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    TaskConfiguration = taskConfiguration;
    logger_           = loggerFactory.CreateLogger<ArmoniKClient>();
  }

  public ObjectPool<ChannelBase> ChannelPool
    => channelPool_ ??= ClientServiceConnector.ControlPlaneConnectionPool(properties_,
                                                                          loggerFactory_);

  public Task<IBlobService> GetBlobService()
  {
    if (blobService_ is not null)
    {
      return Task.FromResult(blobService_);
    }

    blobService_ = BlobServiceFactory.CreateBlobService(ChannelPool,
                                                        loggerFactory_);
    return Task.FromResult(blobService_);
  }

  public Task<ISessionService> GetSessionService()
  {
    if (sessionService_ is not null)
    {
      return Task.FromResult(sessionService_);
    }

    if (TaskConfiguration == null)
    {
      throw new ArgumentNullException(nameof(TaskConfiguration));
    }

    sessionService_ = SessionServiceFactory.CreateSessionService(ChannelPool,
                                                                 properties_,
                                                                 TaskConfiguration,
                                                                 loggerFactory_);
    return Task.FromResult(sessionService_);
  }

  public Task<ISessionService> GetSessionService(TaskConfiguration taskConfiguration)
  {
    if (sessionService_ is not null)
    {
      return Task.FromResult(sessionService_);
    }

    sessionService_ = SessionServiceFactory.CreateSessionService(ChannelPool,
                                                                 properties_,
                                                                 taskConfiguration,
                                                                 loggerFactory_);
    return Task.FromResult(sessionService_);
  }

  public async Task<ITasksService> GetTasksService()
  {
    if (tasksService_ is not null)
    {
      return tasksService_;
    }

    tasksService_ = TasksServiceFactory.CreateTaskService(ChannelPool,
                                                          await GetBlobService(),
                                                          loggerFactory_);
    return tasksService_;
  }

  public Task<IEventsService> GetEventsService()
  {
    if (eventsService_ is not null)
    {
      return Task.FromResult(eventsService_);
    }

    eventsService_ = EventsServiceFactory.CreateEventsService(ChannelPool,
                                                              loggerFactory_);
    return Task.FromResult(eventsService_);
  }

  public Task<BlobHandler> GetBlobHandler(BlobInfo blobInfo)
    => Task.FromResult(new BlobHandler(blobInfo,
                                       this));

  public Task<TaskHandler> GetTaskHandler(TaskInfos taskInfos)
    => Task.FromResult(new TaskHandler(this,
                                       taskInfos));

  public Task<SessionHandler> GetSessionHandler(SessionInfo session)
    => Task.FromResult(new SessionHandler(session,
                                          this));
}
