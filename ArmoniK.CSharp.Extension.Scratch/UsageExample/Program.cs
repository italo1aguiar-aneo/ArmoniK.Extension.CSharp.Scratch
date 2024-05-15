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

namespace UsageExample
{
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
                    new SerilogLoggerProvider(Log.Logger),
                },
                new LoggerFilterOptions().AddFilter("Grpc",
                    LogLevel.Error));
            logger_ = factory.CreateLogger<Program>();
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",
                    true,
                    false)
                .AddEnvironmentVariables();
            _configuration = builder.Build();

            var client = new ArmoniKClient(new Properties(_configuration, null, "http://172.22.89.16"), factory);

            var session = await client.SessionService.CreateSession(new List<string>(["subtasking"]));

            Console.WriteLine($"sessionId: {session.Id}");

            var payload = await client.BlobService.CreateBlobAsync(new BlobInfo("Payload"), session);

            Console.WriteLine($"payloadId: {payload.BlobId}");

            var result = await client.BlobService.CreateBlobAsync(new BlobInfo("Result"), session);

            Console.WriteLine($"resultId: {result.BlobId}");

            var task = await client.TasksService.SubmitTasksAsync(
                new List<TaskNode>([ new TaskNode()
                {
                    Payload = payload,
                    ExpectedOutputs = new[] { result },
                    TaskOptions = new TaskOptions
                        {
                            MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                            MaxRetries  = 2,
                            Priority    = 1,
                            PartitionId = "subtasking",
                            Options =
                            {
                                new MapField<string, string>
                                {
                                    {
                                        "UseCase", "Launch"
                                    },
                                },
                            },
                        },
                }]), session);

            Console.WriteLine($"taskId: {task.Single()}");

            await client.EventsService.WaitForBlobsAsync(new List<BlobInfo>([result]),session);

            var download = await client.BlobService.DownloadBlob(result,
                session,
                CancellationToken.None);
            var stringArray = Encoding.ASCII.GetString(download.Content.Span)
                .Split(new[]
                    {
                        '\n',
                    },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (var retorno in stringArray)
            {
                Console.WriteLine($"{retorno}");
            }
        }
    }
}