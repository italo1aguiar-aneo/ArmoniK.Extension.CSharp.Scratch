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
using Grpc.Core;

namespace ArmoniK.Extension.CSharp.Client.Services;

public class EventsService : IEventsService
{
    public EventsService(ChannelBase channel, ILoggerFactory loggerFactory)
    {
        _eventsClient = new Events.EventsClient(channel);
        _logger = loggerFactory.CreateLogger<EventsService>();
    }

    private readonly ILogger<EventsService> _logger;

    private readonly Events.EventsClient _eventsClient;

    public async Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos, Session session,
        CancellationToken cancellationToken = default)
    {
        await _eventsClient.WaitForResultsAsync(session.Id,
            blobInfos.Select(x => x.BlobId).ToList(),
            cancellationToken);
    }
}