using System;
using System.Net;
using System.IO;
using System.Text;

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
            var request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";

            // Write request body
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(jsonContent);
            }

            // Get the response
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}


