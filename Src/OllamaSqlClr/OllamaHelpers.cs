using System;
using System.IO;
using System.Net;

namespace OllamaSqlClr
{
    public static class OllamaHelper
    {
        public static string ExtractField(string json, string fieldName)
        {
            string key = $"\"{fieldName}\":";
            int keyIndex = json.IndexOf(key);

            if (keyIndex == -1)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in JSON.");
            }

            int startIndex = json.IndexOf('"', keyIndex + key.Length) + 1;
            int endIndex = json.IndexOf('"', startIndex);

            if (startIndex == -1 || endIndex == -1)
            {
                throw new FormatException("Malformed JSON: Could not locate the field value.");
            }

            return json.Substring(startIndex, endIndex - startIndex);
        }

        public static int[] ExtractContextArray(string json)
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
            return Array.ConvertAll(elements, int.Parse);
        }

        public static string CallOllamaService(string prompt, int[] contextArray)
        {
            string apiUrl = "http://127.0.0.1:11434/api/generate";
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

    } // end class OllamaHelper
} // end namespace OllamaSqlClr
