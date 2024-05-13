using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Api.gRPC.V1.Submitter;

namespace ArmoniK.Extension.CSharp.Client.Common.Domain.Session
{
    public class SessionInfo
    {
        public readonly string SessionId;

        public SessionInfo(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
