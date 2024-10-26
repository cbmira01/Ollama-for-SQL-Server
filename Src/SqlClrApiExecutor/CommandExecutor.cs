using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace SqlClrApiExecutor
{
    public class CommandExecutor
    {
        /// <summary>
        /// Performs a general API call on a URL with a request body
        /// </summary>
        /// <param name="apiUrl">The URL of the API that will process the request.</param>
        /// <param name="requestBody">Optional request body.</param>
        /// <returns>Response body as a <see cref="SqlString"/>.</returns>
        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString ExecuteApiCommand(SqlString apiUrl, SqlString requestBody, SqlString requestSize)
        {
            try
            {
                ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, requestBody.Value, "full");
                return new SqlString(ExecuteProcess(psi));
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a single prompt completion request to the specified API URL.
        /// </summary>
        /// <param name="apiUrl">The URL of the API that will process the request.</param>
        /// <param name="ask">The initial prompt or question to be completed.</param>
        /// <param name="body">Additional context or content for the prompt.</param>
        /// <returns>A single prompt completion result as a <see cref="SqlString"/>.</returns>
        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(SqlString apiUrl, SqlString ask, SqlString body)
        {
            try
            {
                var prompt = $"{ask.Value} {body.Value}";

                var requestBodyObject = new
                {
                    model = "llama3.2",
                    prompt,
                    stream = false,
                    n = 1   // number of completions
                };

                var requestBody = JsonConvert.SerializeObject(requestBodyObject);
                Debug.WriteLine("Serialized JSON Request Body: " + requestBody);

                ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, requestBody, "brief");
                return new SqlString(ExecuteProcess(psi));
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to the API for multiple prompt completions and returns them as a table,
        /// feeding back the context array from each previous request to the next one to improve iterated responses.
        /// </summary>
        /// <param name="apiUrl">The URL of the API that will process the request.</param>
        /// <param name="ask">The initial prompt or question to be completed.</param>
        /// <param name="body">Additional context or content for the prompt.</param>
        /// <param name="numCompletions">The number of completions requested from the API.</param>
        /// <returns>An <see cref="IEnumerable"/> of prompt completion results, each as a string.</returns>
        [SqlFunction(
            FillRowMethodName = "FillRow",
            TableDefinition = "Completion NVARCHAR(MAX)"
        )]
        public static IEnumerable<string> CompleteMultiplePrompts(SqlString apiUrl, SqlString ask, SqlString body, SqlInt32 numCompletions)
        {
            try
            {
                var completions = new List<string>();
                int[] contextArray = null; 
                var prompt = $"{ask.Value} {body.Value}";

                Debug.WriteLine($"Number of completions: {(int)numCompletions}, Prompt: {prompt}");

                for (int i = 0; i < numCompletions.Value; i++)
                {
                    var requestBodyObject = new
                    {
                        model = "llama3.2",
                        prompt,
                        stream = false,
                        context = contextArray ?? new int[0]  // Use an empty array for the first request
                    };

                    // Create and run a process for the API call
                    var requestBody = JsonConvert.SerializeObject(requestBodyObject);
                    ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, requestBody, "full");
                    var response = ExecuteProcess(psi);

                    // Extract the new context array from the response for the next request
                    var parsedResponse = ParseResponse(response);
                    contextArray = parsedResponse.Context;

                    // Add the completion text to the list
                    completions.Add(parsedResponse.Completion);
                }

                // Return the list of completions
                return completions;
            }
            catch (Exception ex)
            {
                // In case of an error, return the exception message
                return new[] { ex.Message };
            }
        }

        /// <summary>
        /// A hypothetical method that parses the API response to extract the completion and the context.
        /// </summary>
        /// <param name="response">The raw response from the API call.</param>
        /// <returns>A structure containing the completion text and context array.</returns>
        private static (string Completion, int[] Context) ParseResponse(string response)
        {
            // Parse the response to extract the completion and context array
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

            string completion = jsonResponse.response;
            int[] context = jsonResponse.context?.ToObject<int[]>() ?? new int[0];

            return (completion, context);
        }

        /// <summary>
        /// Fills a row in the result set with a single prompt completion result.
        /// </summary>
        /// <param name="completionObj">The object containing the completion result.</param>
        /// <param name="completion">The output parameter for the row value.</param>
        public static void FillRow(object completionObj, out SqlString completion)
        {
            completion = new SqlString(completionObj.ToString());
        }

        /// <summary>
        /// Creates and configures a ProcessStartInfo instance for executing the external API command.
        /// </summary>
        /// <param name="apiUrl">The URL to be used in the process arguments.</param>
        /// <param name="requestBody">The request body to be passed to the process.</param>
        /// <param name="requestBody">The reqest size, brief or full.</param>
        /// <returns>Configured ProcessStartInfo instance.</returns>
        private static ProcessStartInfo CreateProcessStartInfo(string apiUrl, string requestBody, string requestSize)
        {

#if DEBUG
            var fileName = @"C:\Users\cmirac2\Source\PrivateRepos\ApiCommandSqlExecutor\Src\ApiCommandLineApp\bin\Debug\ApiCommandLineApp.exe";
#else
            var fileName = @"C:\Users\cmirac2\Source\PrivateRepos\ApiCommandSqlExecutor\Src\ApiCommandLineApp\bin\Release\ApiCommandLineApp.exe";
#endif

            var escapedRequestBody = requestBody.Replace("\"", "\\\"");

            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"\"{apiUrl}\" \"{escapedRequestBody}\" \"{requestSize}\" ",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return psi;
        }

        /// <summary>
        /// Executes a process based on the given ProcessStartInfo, enters debug mode, waits for the process to exit, and captures the output.
        /// </summary>
        /// <param name="psi">The ProcessStartInfo to start the process.</param>
        /// <returns>The output from the process.</returns>
        private static string ExecuteProcess(ProcessStartInfo psi)
        {
            using (Process process = new Process())
            {
                process.StartInfo = psi;

                // Start the process
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();

                // Capture the output after the process has exited
                string output = process.StandardOutput.ReadToEnd();

                return output;
            }
        }

    } // end class
} // end namespace
