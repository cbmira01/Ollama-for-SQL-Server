﻿using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using Microsoft.SqlServer.Server;

namespace SqlClrApiExecutor
{
    public class ApiExecutor
    {
        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString GetCompletionOnPrompt(SqlString prompt)
        {
            try
            {
                string result = CallProxyService(prompt.Value);

                // Manually extract the "response" field from the JSON result
                string response = ExtractResponseField(result);
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        private static string ExtractResponseField(string json)
        {
            // Simple parsing logic to extract the value of "response"
            string responseKey = "\"response\":";
            int responseKeyIndex = json.IndexOf(responseKey);

            if (responseKeyIndex == -1)
            {
                return "Error: 'response' field not found";
            }

            // Move index to the start of the value after the key
            int startIndex = responseKeyIndex + responseKey.Length;

            // Find the start of the value, handling potential spaces
            startIndex = json.IndexOf('"', startIndex) + 1;
            int endIndex = json.IndexOf('"', startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                return "Error: Malformed JSON";
            }

            return json.Substring(startIndex, endIndex - startIndex);
        }

        private static string CallProxyService(string prompt)
        {
            string apiUrl = "http://localhost:11434/api/generate";

            // Manually build the JSON string
            string jsonPayload = $"{{\"prompt\": \"{prompt}\", \"model\":\"llama3.2\",\"stream\":false}}";

            // Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "POST";
            request.ContentType = "application/json";

            // Write the JSON payload to the request body
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonPayload);
            }

            // Read and return the response
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    } // end class
} // end namespace
