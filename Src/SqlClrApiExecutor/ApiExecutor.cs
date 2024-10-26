using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using Microsoft.SqlServer.Server;

namespace SqlClrApiExecutor
{
    public class ApiExecutor
    {
        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(
            SqlString askPrompt, 
            SqlString additionalPrompt)
        {
            var prompt = askPrompt + additionalPrompt;

            try
            {
                string result = CallOllamaService(prompt.Value);

                string response = ExtractField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        [SqlFunction(
            FillRowMethodName = "FillRow",
            TableDefinition = "OllamaCompletion NVARCHAR(MAX)"
        )]
        public static IEnumerable<string> CompleteMultiplePrompts(
            SqlString askPrompt, 
            SqlString additionalPrompt, 
            SqlInt32 numCompletions)
        {
            var prompt = askPrompt + additionalPrompt;

            try
            {
                string result = CallOllamaService(prompt.Value);

                string response = ExtractField(result, "response");
                return null; // new SqlString(response);
            }
            catch (Exception ex)
            {
                return new[] { ex.Message };
            }
        }

        public static void FillRow(object completionObj, out SqlString completion)
        {
            completion = new SqlString(completionObj.ToString());
        }

        private static string ExtractField(string json, string fieldName)
        {
            // Construct the search key based on the specified field name
            string key = $"\"{fieldName}\":";
            int keyIndex = json.IndexOf(key);

            if (keyIndex == -1)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in JSON.");
            }

            // Move index to the start of the value after the key
            int startIndex = json.IndexOf('"', keyIndex + key.Length) + 1;
            int endIndex = json.IndexOf('"', startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new FormatException("Malformed JSON: Could not locate the field value.");
            }

            return json.Substring(startIndex, endIndex - startIndex);
        }

        private static string CallOllamaService(string prompt)
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

