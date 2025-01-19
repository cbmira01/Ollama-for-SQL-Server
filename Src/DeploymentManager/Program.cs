using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using DeploymentManager.Commands;

namespace DeploymentManager
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsDict = new Dictionary<string, string>();

            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
            {
                settingsDict[key] = ConfigurationManager.AppSettings[key];
            }

            settingsDict["SqlClrContextConnection"] = ConfigurationManager.ConnectionStrings["SqlClrContextConnection"].ConnectionString;
            settingsDict["SqlServerConnection"] = ConfigurationManager.ConnectionStrings["SqlServerConnection"].ConnectionString;

            settingsDict["RepoRootDirectory"] = FindRepoRoot();
            settingsDict["ScriptsDirectory"] = $"{FindRepoRoot()}\\Src\\DeploymentManager\\Scripts";
            settingsDict["ImagesDirectory"] = $"{FindRepoRoot()}\\Images";

            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("===== Ollama Completions for SQL Server =====");
                Console.WriteLine("============ Deployment Manager =============");
                Console.WriteLine();

                Console.WriteLine("Current configuration settings:");
                Console.WriteLine();
                foreach (var kvp in settingsDict)
                {
                    Console.WriteLine($"    {kvp.Key}:  \"{kvp.Value}\"");
                }

                Console.WriteLine();
                Console.WriteLine("Choose an option:");
                Console.WriteLine();
                Console.WriteLine("       --- Environment checks ---");
                Console.WriteLine("   1. Check external services");
                Console.WriteLine("   2. List models hosted on Ollama");
                Console.WriteLine();
                Console.WriteLine("       --- Perform for initial installation ---");
                Console.WriteLine("   3. Establish the CLR database and its tables");
                Console.WriteLine("   4. Populate configuration and schema data");
                Console.WriteLine("   5. Populate data for demonstrations");
                Console.WriteLine("   6. Populate image table");
                Console.WriteLine();
                Console.WriteLine("       --- Perform after every RELEASE build ---");
                Console.WriteLine("   7. Relink to the CLR assembly and recreate external functions");
                Console.WriteLine();
                Console.WriteLine("       --- Check or revert current deployment ---");
                Console.WriteLine("   8. Check the current deployment");
                Console.WriteLine("   9. Revert the current deployment (drop everything)");
                Console.WriteLine();
                Console.WriteLine("   0. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CheckExternalServicesCommand.Execute(settingsDict);
                        break;

                    case "2":
                        ListHostedModelsCommand.Execute(settingsDict);
                        break;

                    case "3":
                        RunScript.Execute(settingsDict, "establish-clr-database.sql");
                        break;

                    case "4":
                        RunScript.Execute(settingsDict, "populate-config-and-schema.sql");
                        break;

                    case "5":
                        RunScript.Execute(settingsDict, "populate-demo-data.sql");
                        break;

                    case "6":
                        LoadImageFiles.Execute(settingsDict);
                        break;

                    case "7":
                        RunScript.Execute(settingsDict, "relink-clr-assembly.sql");
                        break;

                    case "8":
                        RunScript.Execute(settingsDict, "check-this-deployment.sql");
                        break;

                    case "9":
                        RunScript.Execute(settingsDict, "revert-this-deployment.sql");
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

        static string FindRepoRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!Directory.Exists(Path.Combine(currentDir, ".git")) &&
                   Directory.GetParent(currentDir) != null)
            {
                currentDir = Directory.GetParent(currentDir).FullName;
            }

            return currentDir;
        }
    }
}
