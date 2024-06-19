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

using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;

namespace ArmoniK.Extension.CSharp.Client.Handlers;

public class SessionHandler : SessionInfo
{
  public readonly ArmoniKClient ArmoniKClient;

  public SessionHandler(SessionInfo   session,
                        ArmoniKClient armoniKClient)
    : base(session.SessionId)
    => ArmoniKClient = armoniKClient;

  public async Task CancelSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.CancelSessionAsync(this,
                                            cancellationToken);
  }

  public async Task CloseSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.CloseSessionAsync(this,
                                           cancellationToken);
  }

  public async Task PauseSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.PauseSessionAsync(this,
                                           cancellationToken);
  }

  public async Task StopSubmission(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.StopSubmissionAsync(this,
                                             cancellationToken);
  }

  public async Task ResumeSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.ResumeSessionAsync(this,
                                            cancellationToken);
  }

  public async Task PurgeSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.PurgeSessionAsync(this,
                                           cancellationToken);
  }

  public async Task DeleteSession(CancellationToken cancellationToken)
  {
    var sessionService = await ArmoniKClient.GetSessionService();
    await sessionService.DeleteSessionAsync(this,
                                            cancellationToken);
  }
}
