using System;
using DeploymentManager.Commands;
using Configuration;

namespace DeploymentManager
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                UI.WriteColoredLine("===== Ollama Completions for SQL Server =====", ConsoleColor.Cyan, newLine: true);
                UI.WriteColoredLine("============ Deployment Manager =============", ConsoleColor.Cyan, newLine: true);

                Console.WriteLine();
                UI.WriteColoredLine("Current configuration settings", ConsoleColor.Cyan, newLine: true);
                Console.WriteLine();
                WriteSymbolInColor("ApiUrl");
                WriteSymbolInColor("QueryProductionRetryLimit");
                WriteSymbolInColor("ApiTimeoutMs");
                WriteSymbolInColor("SqlClrContextConnection");
                WriteSymbolInColor("SqlServerConnection");
                WriteSymbolInColor("RepoRootDirectory");
                WriteSymbolInColor("ScriptsDirectory");
                WriteSymbolInColor("ImagesDirectory");

                Console.WriteLine();
                UI.WriteColoredLine("Choose an option:", ConsoleColor.Cyan, newLine: true);
                Console.WriteLine();
                UI.WriteColoredLine("         --- Environment checks ---", ConsoleColor.Yellow, newLine: true);
                Console.WriteLine("   1. Check external services");
                Console.WriteLine("   2. List models hosted on Ollama");
                Console.WriteLine();
                UI.WriteColoredLine("         --- Perform for initial installation ---", ConsoleColor.Yellow, newLine: true);
                Console.WriteLine("   3. Establish the 'AI_Lab' database, its tables and CLR permissions");
                Console.WriteLine("   4. Populate complex prompts and database schema");
                Console.WriteLine("   5. Populate demonstration tables");
                Console.WriteLine("   6. Populate the images table");
                Console.WriteLine();
                UI.WriteColoredLine("         --- Perform after every RELEASE build ---", ConsoleColor.Yellow, newLine: true);
                Console.WriteLine("   7. Relink CLR assembly, recreate functions, run sanity check");
                Console.WriteLine();
                UI.WriteColoredLine("         --- Check or revert current deployment ---", ConsoleColor.Yellow, newLine: true);
                Console.WriteLine("   8. Check the current deployment");
                Console.WriteLine("   9. Revert the current deployment, drop the [AI_Lab] database");
                Console.WriteLine();
                Console.WriteLine("   0. Exit");
                UI.WriteColoredLine("Enter your choice: ", ConsoleColor.Cyan, newLine: false);

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CheckExternalServices.Execute();
                        break;

                    case "2":
                        ListHostedModels.Execute();
                        break;

                    case "3":
                        RunScript.Execute("establish-clr-database.sql");
                        break;

                    case "4":
                        RunScript.Execute("populate-config-and-schema.sql");
                        break;

                    case "5":
                        RunScript.Execute("populate-demo-data.sql");
                        break;

                    case "6":
                        LoadImageFiles.Execute();
                        break;

                    case "7":
                        RunScript.Execute("relink-clr-assembly.sql");
                        break;

                    case "8":
                        RunScript.Execute("check-this-deployment.sql");
                        break;

                    case "9":
                        RunScript.Execute("revert-this-deployment.sql");
                        break;

                    case "0":
                        Console.WriteLine("Exiting Deployment Manager. Goodbye!");
                        return;

                    default:
                        Console.WriteLine("Invalid choice, press any key to try again...");
                        Console.ReadKey();
                        continue;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
            }
        }

        private static void WriteSymbolInColor(string symbolName)
        {
            var pad = 30;
            var symbolValue = AppConfig.GetSymbolValue(symbolName).ToString();

            // Print symbolName right-justified in a field of 35 characters
            string formattedSymbolName = symbolName.PadLeft(pad);
            UI.WriteColoredLine(formattedSymbolName, ConsoleColor.Green, newLine: false);

            // Move cursor to position 38 and print symbolValue
            Console.SetCursorPosition(pad+1, Console.CursorTop);
            UI.WriteColoredLine(symbolValue, ConsoleColor.White, newLine: true);
        }
    }
}
