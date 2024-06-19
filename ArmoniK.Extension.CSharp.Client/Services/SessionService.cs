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

using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class SessionService : ISessionService
{
  private readonly ObjectPool<ChannelBase> _channel;
  private readonly ILogger<SessionService> _logger;

  private readonly Properties _properties;

  public SessionService(ObjectPool<ChannelBase> channel,
                        Properties              properties,
                        ILoggerFactory          loggerFactory)
  {
    _properties = properties;
    _logger     = loggerFactory.CreateLogger<SessionService>();
    _channel    = channel;
  }

  public async Task<SessionInfo> CreateSessionAsync(CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    var createSessionReply = await sessionClient.CreateSessionAsync(new CreateSessionRequest
                                                                    {
                                                                      DefaultTaskOption = _properties.TaskOptions.ToTaskOptions(),
                                                                      PartitionIds =
                                                                      {
                                                                        _properties.PartitionIds,
                                                                      },
                                                                    });

    return new SessionInfo(createSessionReply.SessionId);
  }

  public async Task CancelSessionAsync(SessionInfo       session,
                                       CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.CancelSessionAsync(new CancelSessionRequest
                                           {
                                             SessionId = session.SessionId,
                                           });
  }

  public async Task CloseSessionAsync(SessionInfo       session,
                                      CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.CloseSessionAsync(new CloseSessionRequest
                                          {
                                            SessionId = session.SessionId,
                                          });
  }

  public async Task PauseSessionAsync(SessionInfo       session,
                                      CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.PauseSessionAsync(new PauseSessionRequest
                                          {
                                            SessionId = session.SessionId,
                                          });
  }

  public async Task StopSubmissionAsync(SessionInfo       session,
                                        CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.StopSubmissionAsync(new StopSubmissionRequest
                                            {
                                              SessionId = session.SessionId,
                                            });
  }

  public async Task ResumeSessionAsync(SessionInfo       session,
                                       CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.ResumeSessionAsync(new ResumeSessionRequest
                                           {
                                             SessionId = session.SessionId,
                                           });
  }

  public async Task PurgeSessionAsync(SessionInfo       session,
                                      CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.PurgeSessionAsync(new PurgeSessionRequest
                                          {
                                            SessionId = session.SessionId,
                                          });
  }

  public async Task DeleteSessionAsync(SessionInfo       session,
                                       CancellationToken cancellationToken = default)
  {
    await using var channel       = await _channel.GetAsync(cancellationToken);
    var             sessionClient = new Sessions.SessionsClient(channel);
    await sessionClient.DeleteSessionAsync(new DeleteSessionRequest
                                           {
                                             SessionId = session.SessionId,
                                           });
  }
}
