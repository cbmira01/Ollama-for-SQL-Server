using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace ApiCommandLineApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ApiCommandLineApp <url> <requestBody>");
                return;
            }

            string apiUrl = args[0];
            string requestBody = args[1];

            //string apiUrl = "http://localhost:11434/api/generate";
            //string apiUrl = "https://dogapi.dog/api/facts?number=5";

            //var requestBody = new
            //{
            //    model = "llama3.2",
            //    prompt = "Why is the sky blue?",
            //    stream =  false,
            //    n = 1
            //};

            //string json = JsonConvert.SerializeObject(requestBody);

            string json = requestBody.Replace("\\\"", "\"");

            try
            {
                string response = PostToApi(apiUrl, json);
                Console.WriteLine(response);
#if DEBUG
                Console.ReadKey();
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
#if DEBUG
                Console.ReadKey();
#endif
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
