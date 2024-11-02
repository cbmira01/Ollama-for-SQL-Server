﻿using JsonClrLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace OllamaSqlClr
{
    public static class OllamaHelpers
    {
        public static string ApiGenerateUrl { get; set; } = "http://127.0.0.1:11434/api/generate";
        public static string ApiTagsUrl { get; set; } = "http://127.0.0.1:11434/api/tags";
        public static int RequestTimeout { get; set; } = 100000; // Default timeout of 100 seconds

        public static List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt,
            string modelName)
        {
            return GetModelResponseToPrompt(prompt, modelName, null);
        }

        public static List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt, 
            string modelName, 
            List<int> context)
        {
            var data = new List<KeyValuePair<string, object>>
            {
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateArray("context", context)
            };

            string json = JsonSerializerDeserializer.Serialize(data);

#if DEBUG
            Console.WriteLine("Request...");
            JsonSerializerDeserializer.DumpJson(json);
#endif

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiGenerateUrl);
            request.Timeout = RequestTimeout;
            request.Method = "POST";
            request.ContentType = "application/json";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(json);
            }

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }

#if DEBUG
            Console.WriteLine("Response...");
            JsonSerializerDeserializer.DumpJson(responseJson);
#endif

            return JsonSerializerDeserializer.Deserialize(responseJson);
        }

        public static List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiTagsUrl);
            request.Timeout = RequestTimeout;
            request.Method = "GET";

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }

#if DEBUG
            Console.WriteLine("Response...");
            JsonSerializerDeserializer.DumpJson(responseJson);
#endif

            return JsonSerializerDeserializer.Deserialize(responseJson);
        }

    } // end class OllamaHelpers
} // end namespace OllamaSqlClr
