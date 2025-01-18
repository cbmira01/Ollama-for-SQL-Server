using System;
using System.Collections.Generic;

namespace DeploymentManager.Commands
{
    public static class ListHostedModelsCommand
    {
        public static void Execute(Dictionary<string, string> settings)
        {
            Console.WriteLine();
            Console.WriteLine("List of hosted models on Ollama...");
            Console.WriteLine();

            try
            {
                // API GET to get hosted models
                // Display the list of models, by an index number
                // User can choose a model by index, or exit
                // User can compose a prompt, or use a default prompt
                // Default prompt: "Can SQL Server and Ollama work together? Answer briefly."
                // API POST the request, get a response
                // Display the response
                // Exit
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
