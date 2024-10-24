using System;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiCommandLineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length < 2)
            //{
            //    Console.WriteLine("Usage: ApiCommandLineApp <url> <requestBody>");
            //    return;
            //}

            //string apiUrl = args[0];
            //string requestBody = args[1];

            string apiUrl = "http://localhost:11434";
            string requestBody = ""; // blank requestBody for now

            try
            {
                string response = PostToApi(apiUrl, requestBody);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static string PostToApi(string apiUrl, string jsonContent)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(apiUrl);
                request.Method = "POST";
                request.ContentType = "application/json";

                // Add timeouts to prevent indefinite blocking
                request.Timeout = 10000;  // 10 seconds timeout
                request.ReadWriteTimeout = 10000;  // 10 seconds for read/write

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
                            return streamReader.ReadToEnd();
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
                var msg = $"Web Exception: {webEx.Message}";
                Console.WriteLine(msg);

                if (webEx.Response is HttpWebResponse response)
                {
                    Console.WriteLine($"HTTP Status: {response.StatusCode}");
                }
                else
                {
                    Console.WriteLine("No response received");
                }
                Console.WriteLine(webEx.StackTrace);

                return msg;

            }
            catch (NotSupportedException ex)
            {
                var msg = $"Not Supported Exception: {ex.Message}";
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
