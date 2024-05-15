using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ArmoniK.Extension.CSharp.Client.Common.Services
{
    public interface IEventService
    {
        Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos, Session session, CancellationToken cancellationToken = default);
    }
}
