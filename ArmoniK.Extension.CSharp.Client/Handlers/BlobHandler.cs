using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Handlers;

namespace ArmoniK.Extension.CSharp.Client.Handlers
{
    public class BlobHandler: BlobHandlerBase
    {
        public BlobHandler(string name, string id, string sessionId, ArmoniKClient armoniKClient) : base(name, id, sessionId, armoniKClient)
        {
        }

        public override IAsyncEnumerable<byte[]> DownloadBlobData()
        {
            var blobClient = ArmoniKClient.GetBlobService();

            throw new NotImplementedException();
        }

        public override Task UploadBlobData(IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent)
        {
            throw new NotImplementedException();
        }

        public override Task UploadBlobData(ReadOnlyMemory<byte> blobContent)
        {
            throw new NotImplementedException();
        }
    }
}
