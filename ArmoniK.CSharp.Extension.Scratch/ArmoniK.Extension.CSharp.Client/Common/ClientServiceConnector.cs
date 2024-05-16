using ArmoniK.Api.Client.Options;
using ArmoniK.Api.Client.Submitter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using ArmoniK.Utils;
using Grpc.Core;
using ArmoniK.Api.gRPC.V1;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace ArmoniK.Extension.CSharp.Client.Common
{
    /// <summary>
    ///   ClientServiceConnector is the class to connection to the control plane with different
    ///   like address,port, insecure connection, TLS, and mTLS
    /// </summary>
    public class ClientServiceConnector
    {
        /// <summary>
        ///   Create a connection pool to the control plane with mTLS authentication
        /// </summary>
        /// <param name="properties">Configuration Properties</param>
        /// <param name="loggerFactory">Optional logger factory</param>
        /// <returns>The connection pool</returns>
        public static ObjectPool<ChannelBase> ControlPlaneConnectionPool(Properties properties,
            ILoggerFactory loggerFactory = null)
        {
            var options = new GrpcClient
            {
                AllowUnsafeConnection = !properties.ConfSslValidation,
                CaCert = properties.CaCertFilePem,
                CertP12 = properties.ClientP12File,
                CertPem = properties.ClientCertFilePem,
                KeyPem = properties.ClientKeyFilePem,
                Endpoint = properties.ControlPlaneUri.ToString(),
                OverrideTargetName = properties.TargetNameOverride,
            };

            if (properties.ControlPlaneUri.Scheme == Uri.UriSchemeHttps && options.AllowUnsafeConnection &&
                string.IsNullOrEmpty(options.OverrideTargetName))
            {
#if NET5_0_OR_GREATER
                var doOverride = !string.IsNullOrEmpty(options.CaCert);
#else
                var doOverride = true;
#endif
                if (doOverride)
                {
                    // Doing it here once to improve performance
                    options.OverrideTargetName = GrpcChannelFactory.GetOverrideTargetName(options,
                        GrpcChannelFactory.GetServerCertificate(properties.ControlPlaneUri,
                            options)) ?? "";
                }
            }


            return new ObjectPool<ChannelBase>(() => GrpcChannelFactory.CreateChannel(options,
                loggerFactory?.CreateLogger(typeof(ClientServiceConnector))));
        }
    }
}