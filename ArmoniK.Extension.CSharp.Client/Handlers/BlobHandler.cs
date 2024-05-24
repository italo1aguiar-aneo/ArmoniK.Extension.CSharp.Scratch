using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Services;

namespace ArmoniK.Extension.CSharp.Client.Handlers
{
    public class BlobHandler: BlobInfo
    {
        public BlobHandler(string name, string id, string sessionId, ArmoniKClient armoniKClient) : base(name, id, sessionId)
        {
            ArmoniKClient = armoniKClient;
        }

        public readonly ArmoniKClient ArmoniKClient;

        public async IAsyncEnumerable<byte[]> DownloadBlobData([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var blobClient = await ArmoniKClient.GetBlobService();

            await foreach (var chunk in blobClient.DownloadBlobAsync(this, cancellationToken).ConfigureAwait(false))
            {
                yield return chunk;
            }
        }

        public Task UploadBlobData(IAsyncEnumerable<ReadOnlyMemory<byte>> blobContent)
        {
            throw new NotImplementedException();
        }

        public Task UploadBlobData(ReadOnlyMemory<byte> blobContent)
        {
            throw new NotImplementedException();
        }
    }
}
