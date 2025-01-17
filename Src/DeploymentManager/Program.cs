using System;
using DeploymentManager.Commands;

namespace DeploymentManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "your-connection-string-here";
            string scriptsDirectory = @"Scripts";

            while (true)
            {
                Console.Clear();
                Console.WriteLine("===== Ollama Completions for SQL Server =====");
                Console.WriteLine("============ Deployment Manager =============");
                Console.WriteLine();
                Console.WriteLine($"Respository root directory: {null}");
                Console.WriteLine();
                Console.WriteLine("Choose an option:");
                Console.WriteLine();
                Console.WriteLine("--- Environment checks ---");
                Console.WriteLine("   1. Check external services");
                Console.WriteLine("   2. List models hosted on Ollama");
                Console.WriteLine();
                Console.WriteLine("--- Perform for initial installation ---");
                Console.WriteLine("   3. Establish the CLR database");
                Console.WriteLine("   4. Establish empty tables");
                Console.WriteLine("   5. Populate configuration and schema data");
                Console.WriteLine("   6. Populate data for demonstrations");
                Console.WriteLine();
                Console.WriteLine("--- Perform after every RELEASE build ---");
                Console.WriteLine("   7. Relink to the CLR assembly and recreate external functions");
                Console.WriteLine();
                Console.WriteLine("--- Check or revert current deployment ---");
                Console.WriteLine("   8. Check the current deployment");
                Console.WriteLine("   9. Revert the current deployment (drop everything)");
                Console.WriteLine();
                Console.WriteLine("   0. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();
                ICommand command = null;

                switch (choice)
                {
                    case "1":
                        command = new SetUpDatabaseCommand(connectionString, $"{scriptsDirectory}/setup-database.sql");
                        break;
                    //case "2":
                    //    command = new SetUpCLRAssembliesCommand(connectionString, $"{scriptsDirectory}/setup-clr.sql");
                    //    break;
                    //case "3":
                    //    command = new CreateTablesCommand(connectionString, $"{scriptsDirectory}/create-tables.sql");
                    //    break;
                    //case "4":
                    //    command = new LoadDataCommand(connectionString, $"{scriptsDirectory}/load-data.sql");
                    //    break;
                    //case "5":
                    //    command = new RunExperiment1Command(connectionString, $"{scriptsDirectory}/experiment1.sql");
                    //    break;
                    //case "6":
                    //    command = new RunExperiment2Command(connectionString, $"{scriptsDirectory}/experiment2.sql");
                    //    break;
                    case "0":
                        Console.WriteLine("Exiting Deployment Manager. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice, press any key to try again...");
                        Console.ReadKey();
                        continue;
                }

                Console.Clear();
                command.Execute();

                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
            }
        }
    }
}
