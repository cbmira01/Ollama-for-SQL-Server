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
                Console.WriteLine("===== Ollama Completions for SQL Server =====");
                Console.WriteLine("============ Deployment Manager =============");

                Console.WriteLine();
                Console.WriteLine("Current configuration settings:");
                Console.WriteLine();
                Console.WriteLine($"    ApiUrl:  \"{AppConfig.ApiUrl}\"");
                Console.WriteLine($"    QueryProductionRetryLimit:  \"{AppConfig.QueryProductionRetryLimit}\"");
                Console.WriteLine($"    ApiTimeoutMs:  \"{AppConfig.ApiTimeoutMs}\"");
                Console.WriteLine($"    SqlClrContextConnection:  \"{AppConfig.SqlClrContextConnection}\"");
                Console.WriteLine($"    SqlServerConnection:  \"{AppConfig.SqlServerConnection}\"");
                Console.WriteLine($"    RepoRootDirectory:  \"{AppConfig.RepoRootDirectory}\"");
                Console.WriteLine($"    ScriptsDirectory:  \"{AppConfig.ScriptsDirectory}\"");
                Console.WriteLine($"    ImagesDirectory:  \"{AppConfig.ImagesDirectory}\"");

                Console.WriteLine();
                Console.WriteLine("Choose an option:");
                Console.WriteLine();
                Console.WriteLine("         --- Environment checks ---");
                Console.WriteLine("   1. Check external services");
                Console.WriteLine("   2. List models hosted on Ollama");
                Console.WriteLine();
                Console.WriteLine("         --- Perform for initial installation ---");
                Console.WriteLine("   3. Establish the 'AI_Lab' database, its tables and CLR permissions");
                Console.WriteLine("   4. Populate complex prompts and database schema");
                Console.WriteLine("   5. Populate demonstration tables");
                Console.WriteLine("   6. Populate the images table");
                Console.WriteLine();
                Console.WriteLine("         --- Perform after every RELEASE build ---");
                Console.WriteLine("   7. Relink CLR assembly, recreate functions, run sanity check");
                Console.WriteLine();
                Console.WriteLine("         --- Check or revert current deployment ---");
                Console.WriteLine("   8. Check the current deployment");
                Console.WriteLine("   9. Revert the current deployment (drop everything)");
                Console.WriteLine();
                Console.WriteLine("   0. Exit");
                Console.Write("Enter your choice: ");

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
    }
}
