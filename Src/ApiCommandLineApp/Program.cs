using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ApiCommandLineApp
{
    class Program
    {
        static void Main(string[] args)
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
                string response = PostToApi(apiUrl, json, responseSize);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static string PostToApi(string apiUrl, string jsonContent, string responseSize)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Add timeouts to prevent indefinite blocking
                request.Timeout = 20000;  // 20 seconds timeout
                request.ReadWriteTimeout = 20000;  // 20 seconds for read/write

                // Write request body
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(jsonContent);
                    streamWriter.Flush();  // Ensure the data is fully written
                }

                // Get the response
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = streamReader.ReadToEnd();

                            // Parse the JSON response
                            var parsedJson = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                            // Check the responseSize argument and return accordingly
                            if (responseSize.Equals("brief", StringComparison.OrdinalIgnoreCase))
                            {
                                // Create a new JSON object with only the 'response' field
                                var briefResponse = new
                                {
                                    response = parsedJson.response != null ? parsedJson.response.ToString() : "Field 'response' not found"
                                };

                                // Return the brief response as a JSON string
                                return JsonConvert.SerializeObject(briefResponse);
                            }
                            else
                            {
                                // Return full JSON response for 'full'
                                return jsonResponse;
                            }
                        }
                    }
                    else
                    {
                        return $"Error: {response.StatusCode}";
                    }
                }
            }
            catch (WebException webEx)
            {
                var msg = $"WebException Message: {webEx.Message}";
                Console.WriteLine(msg);

                if (webEx.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {webEx.InnerException.Message}");
                }

                if (webEx.Status == WebExceptionStatus.ConnectFailure)
                {
                    Console.WriteLine("Connection failed. Check if the API is running and accessible.");
                }

                if (webEx.Response is HttpWebResponse response)
                {
                    Console.WriteLine($"HTTP Status Code: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine("No response received from the server.");
                }

                return msg;
            }
            catch (NotSupportedException ex)
            {
                var msg = $"NotSupportedException: {ex.Message}";
                Console.WriteLine(msg);
                Console.WriteLine(ex.StackTrace);
                return msg;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

    }
}
