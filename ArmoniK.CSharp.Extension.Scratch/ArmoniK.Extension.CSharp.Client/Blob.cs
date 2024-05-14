using Google.Protobuf;

namespace ArmoniK.Extension.CSharp.Client
{
    public class Blob
    {
        public Blob(string blobName, string blobId)
        {
            Name = blobName;
            BlobId = blobId;
        }

        public string Name { get; private set; }
        public string BlobId { get; private set; }
        public ByteString Content { get; private set; }

        public void AddContent(ByteString content)
        {
            //add validations
            Content = content;
        }
    }
}