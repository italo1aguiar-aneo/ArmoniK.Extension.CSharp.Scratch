using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;

namespace ArmoniK.Extension.CSharp.Client.Common.Services
{
    public interface ISessionService
    {
        Task<Session> CreateSession(IEnumerable<string> partitionIds);
    }
}