using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Services;
using ArmoniK.Utils;
using Grpc.Core;

namespace ArmoniK.Extension.CSharp.Client.Factory;

public class BlobServiceFactory
{
    public static IBlobService CreateBlobService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory = null)
    {
        return new BlobService(channel, loggerFactory);
    }
}