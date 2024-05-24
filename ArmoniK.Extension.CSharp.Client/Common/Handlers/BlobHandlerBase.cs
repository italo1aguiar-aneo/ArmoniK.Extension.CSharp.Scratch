using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;

namespace ArmoniK.Extension.CSharp.Client.Common.Handlers
{
    public abstract class BlobHandlerBase: BlobInfo
    {
        protected BlobHandlerBase(string name, string id, string sessionId, ArmoniKClient armoniKClient) : base(name, id, sessionId)
        {
            ArmoniKClient = armoniKClient;
        }

        public readonly ArmoniKClient ArmoniKClient;
        public abstract IAsyncEnumerable<byte[]> DownloadBlobData();
        public abstract Task UploadBlobData(IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent);
        public abstract Task UploadBlobData(ReadOnlyMemory<byte> blobContent);
    }
}
