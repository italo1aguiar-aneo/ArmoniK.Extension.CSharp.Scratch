using System.Threading;
using System.Threading.Tasks;
using ArmoniK.Api.gRPC.V1.Versions;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Versions;
using ArmoniK.Extension.CSharp.Client.Common.Services;
using ArmoniK.Utils;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Extension.CSharp.Client.Services;

internal class VersionsService : IVersionsService
{
    private readonly ObjectPool<ChannelBase> _channel;

    private readonly ILogger<VersionsService> _logger;

    public VersionsService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channel = channel;
        _logger = loggerFactory.CreateLogger<VersionsService>();
    }

    public async Task<VersionsInfo> GetVersion(CancellationToken cancellationToken)
    {
        await using var channel = await _channel.GetAsync(cancellationToken).ConfigureAwait(false);
        var versionClient = new Versions.VersionsClient(channel);

        var listVersionsResponse =
            await versionClient.ListVersionsAsync(new ListVersionsRequest(), cancellationToken: cancellationToken);

        return new VersionsInfo
        {
            Api = listVersionsResponse.Api,
            Core = listVersionsResponse.Core
        };
    }
}