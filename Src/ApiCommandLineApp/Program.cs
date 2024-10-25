using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiCommandLineApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
#if DEBUG
            // This will prompt to attach a debugger when the process starts
            Debugger.Launch();
#endif
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: ApiCommandLineApp <url> <requestBody> <brief/full>");
                return;
            }

            string apiUrl = args[0];
            string requestBody = args[1];
            string responseSize = args[2];

            string json = requestBody.Replace("\\\"", "\"");

            try
            {
                string response = await PostToApiAsync(apiUrl, json, responseSize);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task<string> PostToApiAsync(string apiUrl, string jsonContent, string responseSize)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // Set timeout

                    var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Ensure the response is successful (throws if not)
                    response.EnsureSuccessStatusCode();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var parsedJson = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    // Check the responseSize argument and return accordingly
                    if (responseSize.Equals("brief", StringComparison.OrdinalIgnoreCase))
                    {
                        var briefResponse = new
                        {
                            response = parsedJson.response != null ? parsedJson.response.ToString() : "Field 'response' not found"
                        };
                        return JsonConvert.SerializeObject(briefResponse);
                    }
                    else
                    {
                        return jsonResponse; // Return full JSON response for 'full'
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    var msg = $"HttpRequestException: {httpEx.Message}";
                    Console.WriteLine(msg);
                    if (httpEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {httpEx.InnerException.Message}");
                    }
                    return msg;
                }
                catch (TaskCanceledException taskEx) when (taskEx.InnerException is TimeoutException)
                {
                    var msg = $"Request timed out: {taskEx.Message}";
                    Console.WriteLine(msg);
                    return msg;
                }
                catch (Exception ex)
                {
                    var msg = $"Exception: {ex.Message}";
                    Console.WriteLine(msg);
                    return msg;
                }
            }
        }


    } // end class
} // end namespace
