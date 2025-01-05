using JsonClrLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;

namespace OllamaSqlClr.Helpers
{
    public class OllamaApiClient : IOllamaApiClient
    {
        public readonly string _apiUrl;
        private readonly string _apiGenerateUrl;
        private readonly string _apiTagUrl;
        private readonly int _timeout;

        public OllamaApiClient(string apiUrl)
        {
            _apiUrl = apiUrl;
            _apiGenerateUrl = $"{apiUrl}/api/generate";
            _apiTagUrl = $"{apiUrl}/api/tags";
            _timeout = 100000;
        }

        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt,
            string modelName)
        {
            return GetModelResponseToPrompt(prompt, modelName, null);
        }

        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt,
            string modelName,
            List<int> context)
        {
            var data = JsonBuilder.CreateAnonymousObject(
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateArray("context", context)
            );

            string json = JsonHandler.Serialize(data);
#if DEBUG
            Console.WriteLine("Request...");
            JsonHandler.DumpJson(json);
#endif
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_apiGenerateUrl);
            request.Timeout = _timeout;
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
            JsonHandler.DumpJson(responseJson);
#endif
            return JsonHandler.Deserialize(responseJson);
        }

        public List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_apiTagUrl);
            request.Timeout = _timeout;
            request.Method = "GET";

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }
#if DEBUG
            Console.WriteLine("Response...");
            JsonHandler.DumpJson(responseJson);
#endif
            return JsonHandler.Deserialize(responseJson);
        }

        public List<KeyValuePair<string, object>> GetModelResponseToImage(
            string prompt, 
            string modelName, 
            string base64Image)
        {
            var requestPayload = JsonBuilder.CreateAnonymousObject(
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateField("image", base64Image)
            );

            string requestJson = JsonHandler.Serialize(requestPayload);

#if DEBUG
            Console.WriteLine("Request...");
            JsonHandler.DumpJson(requestJson);
#endif
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_apiGenerateUrl);
            request.Timeout = _timeout;
            request.Method = "POST";
            request.ContentType = "application/json";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(requestJson);
            }

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }
#if DEBUG
            Console.WriteLine("Response...");
            JsonHandler.DumpJson(responseJson);
#endif
            return JsonHandler.Deserialize(responseJson);
        }

        // TODO: refactor, build a private method to actually handle calls to the API
    }
}
