using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Common.Abstracts
{
    public abstract class BaseArmoniKClientService<T>
    {
        private ILoggerFactory _loggerFactory;
        protected ILogger<T> _logger { get; }
        public TaskOptions TaskOptions { get; }

        private readonly Properties _properties;

        private ObjectPool<ChannelBase> _channelPool;

        public ObjectPool<ChannelBase> ChannelPool
            => _channelPool ??= ClientServiceConnector.ControlPlaneConnectionPool(_properties,
                _loggerFactory);

        protected BaseArmoniKClientService(Properties properties, ILoggerFactory loggerFactory, TaskOptions taskOptions)
        {
            _loggerFactory = loggerFactory;
            TaskOptions = taskOptions;
            _properties = properties;
            _logger = loggerFactory.CreateLogger<T>();
        }
    }
}