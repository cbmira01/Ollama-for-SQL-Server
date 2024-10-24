using System;
using System.Collections;
using System.Data.SqlTypes;
using System.Diagnostics;
using Microsoft.SqlServer.Server;

public class CommandExecutor
{
    /// <summary>
    /// Performs a general API call on a URL with a request body
    /// </summary>
    /// <param name="apiUrl">The URL of the API that will process the request.</param>
    /// <param name="requestBody">Optional request body.</param>
    /// <returns>Response body as a <see cref="SqlString"/>.</returns>
    [SqlFunction(DataAccess = DataAccessKind.None)]
    public static SqlString ExecuteApiCommand(SqlString apiUrl, SqlString requestBody)
    {
        try
        {
            // Prepare the process to execute the external command
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = @"C:\path\to\ApiCommandLineApp.exe",
                Arguments = $"\"{apiUrl.Value}\" \"{requestBody.Value}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Execute the command and capture the output
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return new SqlString(output);
            }
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
    public static SqlString CompletePrompt(
        SqlString apiUrl, 
        SqlString ask,
        SqlString body)
    {
        try
        {
            // Prepare the process to execute the external command
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = @"C:\path\to\ApiCommandLineApp.exe",
                Arguments = $"\"{apiUrl.Value}\" \"{requestBody.Value}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Execute the command and capture the output
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return new SqlString(output);
            }
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
    public static IEnumerable CompleteMultiplePrompts(
            SqlString apiUrl,
            SqlString ask,
            SqlString body,
            SqlInt32 numCompletions)
    {
        try
        {
            // Prepare the process to execute the external command
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = @"C:\path\to\ApiCommandLineApp.exe",
                Arguments = $"\"{apiUrl.Value}\" \"{requestBody.Value}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Execute the command and capture the output
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return new SqlString(output);
            }
        }
        catch (Exception ex)
        {
            return new SqlString($"Error: {ex.Message}");
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

        // private method

} 
