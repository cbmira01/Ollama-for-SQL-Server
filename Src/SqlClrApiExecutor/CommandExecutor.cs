using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Diagnostics;
using Microsoft.SqlServer.Server;

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
                ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, requestBody.Value, requestSize.Value);
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
                ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, prompt, "brief");
                return new SqlString(ExecuteProcess(psi));
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a request to the API for multiple prompt completions and returns them as a table.
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
        public static IEnumerable CompleteMultiplePrompts(SqlString apiUrl, SqlString ask, SqlString body, SqlInt32 numCompletions)
        {
            try
            {
                var prompt = $"{ask.Value} {body.Value}";
                ProcessStartInfo psi = CreateProcessStartInfo(apiUrl.Value, prompt, "brief");
                string output = ExecuteProcess(psi);

                // Logic to split the output into multiple rows can be added here

                return null; // Placeholder for actual implementation
            }
            catch (Exception ex)
            {
                return ex.Message; // Placeholder for error handling
            }
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

            return new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = $"\"{apiUrl}\" \"{requestBody}\" \"{requestSize}\" ",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
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
