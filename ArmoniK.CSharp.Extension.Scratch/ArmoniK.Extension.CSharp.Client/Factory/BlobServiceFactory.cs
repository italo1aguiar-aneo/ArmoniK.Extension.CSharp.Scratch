using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Services;

namespace ArmoniK.Extension.CSharp.Client.Factory
{
    public class BlobServiceFactory
    {
        public static IBlobService CreateBlobService(Properties props,
            ILoggerFactory loggerFactory = null)
            => new BlobService(props,
                loggerFactory);
    }
}