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

/// <summary>
///   Handles session management operations for an ArmoniK client, providing methods to control the lifecycle of a
///   session.
/// </summary>
public class SessionHandler
{
  private readonly ArmoniKClient armoniKClient_;
  private readonly SessionInfo   sessionInfo_;

  /// <summary>
  ///   Initializes a new instance of the <see cref="SessionHandler" /> class.
  /// </summary>
  /// <param name="session">The session information for managing session-related operations.</param>
  /// <param name="armoniKClient">The ArmoniK client used to perform operations on the session.</param>
  public SessionHandler(SessionInfo   session,
                        ArmoniKClient armoniKClient)
  {
    armoniKClient_ = armoniKClient;
    sessionInfo_   = session;
  }

  /// <summary>
  ///   Cancels the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task CancelAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.CancelSessionAsync(sessionInfo_,
                                            cancellationToken);
  }

  /// <summary>
  ///   Closes the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task CloseAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.CloseSessionAsync(sessionInfo_,
                                           cancellationToken);
  }

  /// <summary>
  ///   Pauses the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task PauseAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.PauseSessionAsync(sessionInfo_,
                                           cancellationToken);
  }

  /// <summary>
  ///   Stops submissions to the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task StopSubmissionAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.StopSubmissionAsync(sessionInfo_,
                                             cancellationToken);
  }

  /// <summary>
  ///   Resumes the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task ResumeAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.ResumeSessionAsync(sessionInfo_,
                                            cancellationToken);
  }

  /// <summary>
  ///   Purges the session asynchronously, removing all data associated with it.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task PurgeAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.PurgeSessionAsync(sessionInfo_,
                                           cancellationToken);
  }

  /// <summary>
  ///   Deletes the session asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
  public async Task DeleteAsync(CancellationToken cancellationToken)
  {
    var sessionService = await armoniKClient_.GetSessionService();
    await sessionService.DeleteSessionAsync(sessionInfo_,
                                            cancellationToken);
  }
}
