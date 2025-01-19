using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using JsonClrLibrary;

namespace DeploymentManager.Commands
{
    public static class ListHostedModelsCommand
    {
        public static void Execute(Dictionary<string, string> settings)
        {
            Console.WriteLine();
            Console.WriteLine("List of models hosted on Ollama...");
            Console.WriteLine();

            try
            {
                string apiUrl = settings["ApiUrl"];
                string apiGenerateUrl = $"{apiUrl}/api/generate";
                string apiTagUrl = $"{apiUrl}/api/tags";

                // Step 1: Get the list of hosted models
                var models = GetModels(apiTagUrl);

                if (models == null || models.Count == 0)
                {
                    Console.WriteLine("    No models are currently hosted on Ollama.");
                    return;
                }

                // Step 2: Display the list of models
                Console.WriteLine("    Available Models:");
                
                for (int i = 0; i < models.Count; i++)
                {
                    Console.WriteLine($"        {i+1}  Name: {models[i].Item1}, Modified: {models[i].Item2}");
                }

                // Step 3: Let the user select a model
                Console.WriteLine();
                Console.Write("Enter the number of the model to use (or 0 to exit): ");
                if (!int.TryParse(Console.ReadLine(), out int modelIndex) || modelIndex < 1 || modelIndex > models.Count)
                {
                    Console.WriteLine("Exiting...");
                    return;
                }

                string selectedModel = models[modelIndex - 1].Item1;
                Console.WriteLine($"Selected Model: {selectedModel}");

                // Step 4: Compose a prompt
                Console.WriteLine();
                string defaultPrompt = "Who are you and what can you do? Answer briefly.";
                Console.WriteLine($"Default prompt: {defaultPrompt}");
                Console.Write("Enter your prompt (or press Enter to use the default): ");
                string prompt = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(prompt))
                {
                    prompt = defaultPrompt;
                }

                // Step 5: Send the prompt to the selected model
                string response = PostPrompt(apiGenerateUrl, selectedModel, prompt);

                if (!string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("Model Response:");
                    Console.WriteLine(response);
                }
                else
                {
                    Console.WriteLine("No response received from the model.");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static List<(string, string)> GetModels(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 100000;

                string responseJson;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    responseJson = reader.ReadToEnd();
                }
                var result = JsonHandler.Deserialize(responseJson);

                var availableModels = new List<(string, string)>();
                var modelCount = JsonHandler.GetIntegerByPath(result, "models.length");

                for (var i = 0; i < modelCount; i++)
                {
                    var fullName = JsonHandler.GetStringByPath(result, $"models[{i}].name");
                    var name = fullName.Split(':')[0];
                    var modifiedAt = JsonHandler.GetStringByPath(result, $"models[{i}].modified_at");
                    availableModels.Add((name, modifiedAt));
                }

                return availableModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching models: {ex.Message}");
                return null;
            }
        }

        private static string PostPrompt(string url, string modelName, string prompt)
        {
            try
            {
                var requestObject = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("model", modelName),
                    JsonBuilder.CreateField("prompt", prompt),
                    JsonBuilder.CreateField("stream", false)
                );

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Timeout = 100000;
                request.ContentType = "application/json";

                string postData = JsonHandler.Serialize(requestObject);

                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(postData);
                }

                string responseJson;
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    responseJson = reader.ReadToEnd();
                }
                var result = JsonHandler.Deserialize(responseJson);
                string modelResponse = JsonHandler.GetStringField(result, "response");

                return modelResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending prompt: {ex.Message}");
                return null;
            }
        }
    }
}
