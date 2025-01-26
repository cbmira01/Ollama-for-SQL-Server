using System;
using System.Collections.Generic;
using System.Linq;

namespace DeploymentManager.UI
{
    public static class UI
    {
        // Private list of messages from SQL Server to skip
        private static List<string> _nonHelpfulMessages = new List<string>
        {
            "Run the RECONFIGURE statement to install"
        };

        // Moved to a normal static field (no "readonly")
        private static Dictionary<string, ConsoleColor> _keywordColors = new Dictionary<string, ConsoleColor>
        {
            { "[ERROR]", ConsoleColor.Red },
            { "[SYMBOL]", ConsoleColor.DarkYellow },
            { "[CHECK]", ConsoleColor.DarkYellow },
            { "[STEP]", ConsoleColor.DarkCyan }
        };

        public static bool PromptUserConfirmation(
            string promptMessage = "Continue?  N // ", 
            string defaultResponse = "N")
        {
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
            bool insertBlankLine = true)
        {
            if (insertBlankLine)
                Console.WriteLine();

            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{prefix}{message}");
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        public static void WriteKeywordColoredMessage(string message)
        {
            // Skip any messages matching the "non-helpful" list
            if (_nonHelpfulMessages.Any(skipText => message.Contains(skipText)))
            {
                return;
            }

            var matchedKeyword = _keywordColors
                .Keys
                .FirstOrDefault(k => message.Contains(k));

            if (matchedKeyword != null)
            {
                var color = _keywordColors[matchedKeyword];
                WriteColoredLine(message, color);
            }
            else
            {
                WriteColoredLine(message, ConsoleColor.DarkGray, "[INFO]: ");
            }
        }
    }
}
