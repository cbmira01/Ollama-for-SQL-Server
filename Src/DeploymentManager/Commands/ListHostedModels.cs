using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using JsonClrLibrary;
using Configuration;
using DeploymentManager;

namespace DeploymentManager.Commands
{
    public static class ListHostedModels
    {
        public static void Execute()
        {
            Console.WriteLine();
            Console.WriteLine("List of models now hosted on Ollama...");
            Console.WriteLine();

            try
            {
                string generateEndpointUrl = AppConfig.GenerateEndpointUrl;
                string tagEndpointUrl = AppConfig.TagEndpointUrl;

                // Step 1: Get list of hosted models
                var models = GetModels(tagEndpointUrl);

                if (models == null || models.Count == 0)
                {
                    UI.WriteColoredLine("    No models are currently hosted on Ollama.",
                        ConsoleColor.Red, newLine: true);

                    return;
                }

                // Step 2: Display the list of models
                for (int i = 0; i < models.Count; i++)
                {
                    UI.WriteColoredLine($"        {i + 1}", ConsoleColor.White, newLine: false);
                    UI.WriteColoredLine($"  Name:", ConsoleColor.Green, newLine: false);
                    UI.WriteColoredLine($" {models[i].Item1}", ConsoleColor.White, newLine: false);
                    UI.WriteColoredLine($"  Modified:", ConsoleColor.Green, newLine: false);
                    UI.WriteColoredLine($" {models[i].Item2}", ConsoleColor.White, newLine: true);
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
                string defaultPrompt = "Who are you and what can you do? Answer briefly.";

                Console.WriteLine();
                UI.WriteColoredLine($"Default prompt: \"{defaultPrompt}\"", ConsoleColor.Yellow, newLine: true);

                Console.Write("Enter your prompt (or press Enter to use the default prompt): ");
                string prompt = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(prompt))
                {
                    prompt = defaultPrompt;
                }

                // Step 5: Send the prompt to the selected model
                string response = PostPrompt(generateEndpointUrl, selectedModel, prompt);

                Console.WriteLine();

                if (!string.IsNullOrEmpty(response))
                {
                    UI.WriteColoredLine("Model Response: ", ConsoleColor.Green, newLine: false);
                    Console.WriteLine(response);
                }
                else
                {
                    UI.WriteColoredLine("No response received from the model.", ConsoleColor.Red, newLine: true);
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
                request.Timeout = AppConfig.ApiTimeoutMs;

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
                    var name = JsonHandler.GetStringByPath(result, $"models[{i}].name");
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
                request.Timeout = AppConfig.ApiTimeoutMs;
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
