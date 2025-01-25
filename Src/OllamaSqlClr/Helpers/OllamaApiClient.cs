using JsonClrLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using Configuration;

namespace OllamaSqlClr.Helpers
{
    public class OllamaApiClient : IOllamaApiClient
    {
        public readonly string _apiUrl = AppConfig.ApiUrl;
        private readonly string _generateEndpointUrl = AppConfig.GenerateEndpointUrl;
        private readonly string _tagEndpointUrl = AppConfig.TagEndpointUrl ;
        private readonly int _timeout = AppConfig.ApiTimeoutMs;

        public OllamaApiClient()
        {
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
            var requestObject = JsonBuilder.CreateAnonymousObject(
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateArray("context", context)
            );

            return MakeApiCall(_generateEndpointUrl, "POST", requestObject);
        }

        public List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            return MakeApiCall(_tagEndpointUrl, "GET");
        }

        public List<KeyValuePair<string, object>> GetModelResponseToImage(
            string prompt, 
            string modelName, 
            string base64Image)
        {
            var base64ImageList = new List<string> { base64Image };
            var noContext = new List<int> { };

            var requestObject = JsonBuilder.CreateAnonymousObject(
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateArray("images", base64ImageList),
                JsonBuilder.CreateArray("context", noContext)
            );

            return MakeApiCall(_generateEndpointUrl, "POST", requestObject);
        }

        private List<KeyValuePair<string, object>> MakeApiCall(
            string url,
            string method,
            List<KeyValuePair<string, object>> requestObject = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = _timeout;
            request.Method = method;
            request.ContentType = "application/json";

            if (requestObject != null)
            {
                string json = JsonHandler.Serialize(requestObject);
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(json);
                }
            }

            string responseJson;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }
            return JsonHandler.Deserialize(responseJson);
        }
    }
}
