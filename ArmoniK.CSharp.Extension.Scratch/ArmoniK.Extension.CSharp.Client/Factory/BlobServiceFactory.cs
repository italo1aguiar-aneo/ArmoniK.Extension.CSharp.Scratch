﻿using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Extension.CSharp.Client.Services;
using Grpc.Core;

namespace ArmoniK.Extension.CSharp.Client.Factory
{
    public class BlobServiceFactory
    {
        public static IBlobService CreateBlobService(ChannelBase channel, ILoggerFactory loggerFactory = null)
            => new BlobService(channel, loggerFactory);
    }
}