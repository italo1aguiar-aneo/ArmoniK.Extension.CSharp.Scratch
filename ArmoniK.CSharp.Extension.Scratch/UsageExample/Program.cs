// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-2024. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.


using System.CommandLine;
using System.Text;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extension.CSharp.Client;
using ArmoniK.Extension.CSharp.Client.Common;
using ArmoniK.Extension.CSharp.Client.Common.Domain;
using ArmoniK.Extension.CSharp.Client.Services;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace UsageExample;

internal class Program
{
    private static IConfiguration _configuration;
    private static ILogger<Program> logger_;

    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello Armonik New Extension !");


        Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var factory = new LoggerFactory(new[]
            {
                new SerilogLoggerProvider(Log.Logger)
            },
            new LoggerFilterOptions().AddFilter("Grpc",
                LogLevel.Error));

        logger_ = factory.CreateLogger<Program>();

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var taskOptions = new TaskOptions
        {
            MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
            MaxRetries = 2,
            Priority = 1,
            PartitionId = "subtasking",
            Options =
            {
                new MapField<string, string>
                {
                    {
                        "UseCase", "Launch"
                    }
                }
            }
        };

        var props = new Properties(_configuration, taskOptions, ["subtasking"]);

        var client = new ArmoniKClient(props, factory);

        var sessionService = await client.GetSessionService();

        var blobService = await client.GetBlobService();

        var tasksService = await client.GetTasksService();

        var eventsService = await client.GetEventsService();

        var session = await sessionService.CreateSession();

        Console.WriteLine($"sessionId: {session.Id}");

        var payload = await blobService.CreateBlobAsync("Payload", Encoding.ASCII.GetBytes("Hello"), session);

        Console.WriteLine($"payloadId: {payload.BlobId}");

        var result = await blobService.CreateBlobAsync("Result", session);

        Console.WriteLine($"resultId: {result.BlobId}");

        var task = await tasksService.SubmitTasksAsync(
            new List<TaskNode>([
                new TaskNode()
                {
                    Payload = payload,
                    ExpectedOutputs = new[] { result }
                }
            ]), session);

        Console.WriteLine($"taskId: {task.Single()}");

        await eventsService.WaitForBlobsAsync(new List<BlobInfo>([result]), session);

        var download = await blobService.DownloadBlob(result,
            session,
            CancellationToken.None);
        var stringArray = Encoding.ASCII.GetString(download.Content.Span)
            .Split(new[]
                {
                    '\n'
                },
                StringSplitOptions.RemoveEmptyEntries);

        foreach (var returnString in stringArray) Console.WriteLine($"{returnString}");
    }
}