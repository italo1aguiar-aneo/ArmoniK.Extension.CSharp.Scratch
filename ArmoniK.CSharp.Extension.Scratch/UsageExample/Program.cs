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
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace UsageExample
{
    internal class Program
    {
        /// <summary>
        ///   Method for sending task and retrieving their results from ArmoniK
        /// </summary>
        /// <param name="endpoint">The endpoint url of ArmoniK's control plane</param>
        /// <param name="partition">Partition Id of the matching worker</param>
        /// <returns>
        ///   Task representing the asynchronous execution of the method
        /// </returns>
        /// <exception cref="Exception">Issues with results from tasks</exception>
        /// <exception cref="ArgumentOutOfRangeException">Unknown response type from control plane</exception>
        internal static async Task Run(string endpoint,
                                       string partition)
        {
            // Default task options that will be used by each task if not overwritten when submitting tasks
            var taskOptions = new TaskOptions
            {
                MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                MaxRetries = 2,
                Priority = 1,
                PartitionId = partition,
                Options =
                          {
                            new MapField<string, string>
                            {
                              {
                                "UseCase", "Launch"
                              },
                            },
                          },
            };

            var sessionManager = new SessionManager(endpoint, new List<string>() { partition }, taskOptions);

            sessionManager.StartSession();

            var taskSubmissionResponse = sessionManager.SendTask(partition, UnsafeByteOperations.UnsafeWrap(Encoding.ASCII.GetBytes("Hello")));

            await sessionManager.WaitForResultsAsync(taskSubmissionResponse.ExpectedOutputsDictionary.Values);

            // Download result
            var resultByteArray = await sessionManager.DownloadResult(taskSubmissionResponse.ExpectedOutputsDictionary.Values.FirstOrDefault());

            var stringArray = Encoding.ASCII.GetString(resultByteArray)
                                      .Split(new[]
                                             {
                                         '\n',
                                             },
                                             StringSplitOptions.RemoveEmptyEntries);

            foreach (var result in stringArray)
            {
                Console.WriteLine($"{result}");
            }
        }

        public static async Task<int> Main(string[] args)
        {
            // Define the options for the application with their description and default value
            var endpoint = new Option<string>("--endpoint",
                                              description: "Endpoint for the connection to ArmoniK control plane.",
                                              getDefaultValue: () => "http://172.22.89.16:5001");
            var partition = new Option<string>("--partition",
                                               description: "Name of the partition to which submit tasks.",
                                               getDefaultValue: () => "subtasking");
            // Describe the application and its purpose
            var rootCommand = new RootCommand("SubTasking demo for ArmoniK.\n" + $" It sends a task to ArmoniK in the given partition <{partition.Name}>. " +
                                              "The task creates some subtasks and, for the result with an array of subtasks Ids will be returned. " +
                                              "Then, the client retrieves and prints the result of the task parsing the result.\n" +
                                              $"ArmoniK endpoint location is provided through <{endpoint.Name}>");

            // Add the options to the parser
            rootCommand.AddOption(endpoint);
            rootCommand.AddOption(partition);

            // Configure the handler to call the function that will do the work
            rootCommand.SetHandler(Run,
                                   endpoint,
                                   partition);

            // Parse the command line parameters and call the function that represents the application
            return await rootCommand.InvokeAsync(args);
        }
    }
}
