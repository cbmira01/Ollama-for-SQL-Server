using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using Microsoft.SqlServer.Server;

namespace OllamaSqlClr
{
    public class SqlClrFunctions
    {
        #region "Complete Prompt"

        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(
            SqlString askPrompt, 
            SqlString additionalPrompt)
        {
            var prompt = askPrompt + " " + additionalPrompt;

            try
            {
                string result = CallOllamaService(prompt.Value, null);

                string response = ExtractField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        #endregion

        #region "CompleteMultiplePrompts"

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(
            SqlString askPrompt,
            SqlString additionalPrompt,
            SqlInt32 numCompletions)
        {
            var prompt = askPrompt + " " + additionalPrompt;
            var completions = new List<(Guid, string)>();
            int[] contextArray = null; // Placeholder for context tracking between calls

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    string result = CallOllamaService(prompt.Value, contextArray);
                    string response = ExtractField(result, "response");

                    // Generate a unique GUID for each completion
                    Guid completionGuid = Guid.NewGuid();
                    completions.Add((completionGuid, response));

                    // Update context for next iteration if needed
                    contextArray = ExtractContextArray(result);
                }

                return completions;
            }
            catch (Exception ex)
            {
                return new List<(Guid, string)> { (Guid.Empty, $"Error: {ex.Message}") };
            }
        }

        // Updated FillRow method to output both a GUID and the completion string
        public static void FillRow_CompleteMultiplePrompts(object completionObj, 
            out SqlGuid completionGuid, 
            out SqlString ollamaCompletion)
        {
            var (guid, completion) = ((Guid, string))completionObj;

            completionGuid = new SqlGuid(guid);
            ollamaCompletion = new SqlString(completion);
        }

        #endregion

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

        private static int[] ExtractContextArray(string json)
        {
            string key = "\"context\":";
            int keyIndex = json.IndexOf(key);

            if (keyIndex == -1)
                return null;

            int startIndex = json.IndexOf('[', keyIndex) + 1;
            int endIndex = json.IndexOf(']', startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new FormatException("Malformed JSON: Could not locate the context array.");
            }

            string arrayStr = json.Substring(startIndex, endIndex - startIndex);
            string[] elements = arrayStr.Split(',');
            int[] contextArray = Array.ConvertAll(elements, int.Parse);

            return contextArray;
        }

        private static string CallOllamaService(string prompt, int[] contextArray)
        {
            string apiUrl = "http://localhost:11434/api/generate";
            string context = contextArray != null ? $"[{string.Join(",", contextArray)}]" : "[]";
            string jsonPayload = $"{{\"prompt\": \"{prompt}\", \"model\":\"llama3.2\",\"stream\":false, \"context\": {context}}}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Timeout = 100000; // Use default timeout of 100 seconds
            request.Method = "POST";
            request.ContentType = "application/json";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(jsonPayload);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

    } // end class
} // end namespace

