using ArmoniK.Extension.CSharp.Client.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Extension.CSharp.Client.Common;
using Microsoft.Extensions.Logging;
using ArmoniK.Api.gRPC.V1.Tasks;
using System.Threading.Tasks;
using ArmoniK.Api.Client;
using System.Threading;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Utils;
using Grpc.Core;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class EventsService : IEventsService
{
    public EventsService(ObjectPool<ChannelBase> channel, ILoggerFactory loggerFactory)
    {
        _channel = channel;
        _logger = loggerFactory.CreateLogger<EventsService>();
    }

    private readonly ILogger<EventsService> _logger;

    private readonly ObjectPool<ChannelBase> _channel;

    public Task WaitForBlobsAsync(ICollection<string> blobIds, Session session, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos, Session session,
        CancellationToken cancellationToken = default)
    {
        await using var channel = await _channel.GetAsync(cancellationToken).ConfigureAwait(false);
        var eventsClient = new Events.EventsClient(channel);
        await eventsClient.WaitForResultsAsync(session.Id,
            blobInfos.Select(x => x.BlobId).ToList(),
            cancellationToken);
    }
}