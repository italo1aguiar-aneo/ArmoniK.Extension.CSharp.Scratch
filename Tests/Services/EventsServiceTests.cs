using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extension.CSharp.Client.Common.Domain.Session;
using Grpc.Core;
using Moq;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests.Services;

public class EventsServiceTests
{
    [Test]
    public Task CreateSession_ReturnsNewSessionWithId()
    {
        var responses = new EventSubscriptionResponse
        {
            SessionId = "1234", NewResult = new EventSubscriptionResponse.Types.NewResult
            {
                ResultId = "1234",
                Status = ResultStatus.Completed
            }
        };

        var mockInvoker = new Mock<CallInvoker>();

        var callInvoker =
            mockInvoker.SetupAsyncServerStreamingCallInvokerMock<EventSubscriptionRequest, EventSubscriptionResponse>(
                responses);

        var eventsService = MockHelper.GetEventsServiceMock(callInvoker);
        // Act

        Assert.DoesNotThrowAsync(async () => await eventsService.WaitForBlobsAsync(new SessionInfo("sessionId"),
            new[] { new BlobInfo { BlobName = "", BlobId = "1234", SessionId = "sessionId" } }));
        return Task.CompletedTask;
    }
}