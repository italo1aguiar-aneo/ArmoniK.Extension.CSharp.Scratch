using ArmoniK.Extension.CSharp.Client.Common.Abstracts;
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

namespace ArmoniK.Extension.CSharp.Client.Services
{
    public class EventService : BaseArmoniKClientService<EventService>, IEventService
    {
        public EventService(Properties properties, ILoggerFactory loggerFactory, TaskOptions taskOptions = null) : base(
            properties, loggerFactory, taskOptions ?? ClientServiceConnector.InitializeDefaultTaskOptions())
        {
        }
        private Events.EventsClient _eventsClient;
        private readonly object _lock = new();

        // Property to access the EventsClient
        private async Task<Events.EventsClient> GetEventsClientAsync()
        {
            if (_eventsClient == null)
            {
                await InitializeEventsClientAsync();
            }

            return _eventsClient;
        }

        // Method to initialize the TasksClient
        private async Task InitializeEventsClientAsync()
        {
            if (_eventsClient == null)
            {
                var client = new Events.EventsClient(await ChannelPool.GetAsync());
                lock (_lock)
                {
                    _eventsClient ??= client;
                }
            }
        }

        public async Task WaitForBlobsAsync(ICollection<BlobInfo> blobInfos, Session session, CancellationToken cancellationToken = default)
        {
            var eventClient = await GetEventsClientAsync();
            await eventClient.WaitForResultsAsync(session.Id,
                blobInfos.Select(x => x.BlobId).ToList(),
                cancellationToken);
        }
    }
}