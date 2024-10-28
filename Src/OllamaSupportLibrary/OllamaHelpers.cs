using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace OllamaSupportLibrary
{
    public class OllamaHelpers
    {
        public string apiUrl = "http://localhost:1143";
        public string generateEndpoint = "/api/'generate";
        public string tagsEndpoint = "/api/tags";
        public int timeout = 100000; // 100 seconds, default

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

        private static string CallOllamaTags()
        {
            string apiUrl = "http://localhost:11434/api/tags";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Timeout = 100000; // Default timeout of 100 seconds
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        private static List<(string ModelName, int ParameterSize)> ExtractModelInformation(string json)
        {
            var models = new List<(string, int)>();

            string[] modelEntries = json.Split(new string[] { "}," }, StringSplitOptions.None);
            foreach (var entry in modelEntries)
            {
                // Parse model name
                string nameKey = "\"name\":";
                int nameIndex = entry.IndexOf(nameKey);
                string modelName = null;

                if (nameIndex != -1)
                {
                    int nameStart = entry.IndexOf('"', nameIndex + nameKey.Length) + 1;
                    int nameEnd = entry.IndexOf('"', nameStart);
                    modelName = entry.Substring(nameStart, nameEnd - nameStart);
                }

                // Parse parameter size
                string sizeKey = "\"parameter_size\":";
                int sizeIndex = entry.IndexOf(sizeKey);
                int parameterSize = 0;

                if (sizeIndex != -1)
                {
                    int sizeStart = sizeIndex + sizeKey.Length;
                    int sizeEnd = entry.IndexOf(',', sizeStart);
                    parameterSize = int.Parse(entry.Substring(sizeStart, sizeEnd - sizeStart));
                }

                if (modelName != null)
                {
                    models.Add((modelName, parameterSize));
                }
            }

            return models;
        }

    } // end class
} // end namespace
