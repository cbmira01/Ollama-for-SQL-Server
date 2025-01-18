using System;
using System.Collections.Generic;

namespace DeploymentManager.Commands
{
    public static class RunScript
    {
        public static void Execute(Dictionary<string, string> settings, string scriptName)
        {
            Console.WriteLine();
            Console.WriteLine($"Running script \"{scriptName}\"");
            Console.WriteLine();

            try
            {
                // run the script

                Console.WriteLine("SUCCESS MESSAGE");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

