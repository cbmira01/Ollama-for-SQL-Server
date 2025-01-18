using System;
using System.Collections.Generic;

namespace DeploymentManager.Commands
{
    public static class LoadImageFiles
    {
        public static void Execute(Dictionary<string, string> settings)
        {
            Console.WriteLine();
            Console.WriteLine("Loading image files...");
            Console.WriteLine();

            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

