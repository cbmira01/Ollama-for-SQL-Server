using System;
using System.Collections.Generic;
using System.Linq;

namespace DeploymentManager
{
    public static class UI
    {
        public static bool PromptUserConfirmation()
        {
            string promptMessage = "    Continue?  N // ";
            string defaultResponse = "N";

            Console.Write(promptMessage);
            var response = (Console.ReadLine() ?? defaultResponse).Trim().ToUpper();
            if (response != "Y")
            {
                Console.WriteLine("Operation cancelled.");
                return false;
            }
            return true;
        }

        public static void WriteColoredLine(
            string message, 
            ConsoleColor color, 
            string prefix = "", 
            bool newLine = false)
        {
            try
            {
                Console.ForegroundColor = color;
                Console.Write($"{prefix}{message}");
            }
            finally
            {
                Console.ResetColor();

                if (newLine)
                {
                    Console.WriteLine();
                }
            }
        }
    }
}
