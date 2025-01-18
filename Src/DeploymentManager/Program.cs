using System;
using System.Configuration;
using System.IO;
using DeploymentManager.Commands;

namespace DeploymentManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SqlServerContextConnection"].ConnectionString;

            string repoRootDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));
            string scriptsDirectory = $"{repoRootDirectory}Src\\DeploymentManager\\Scripts\\";
            string imagesDirectory = $"{repoRootDirectory}Images\\";

            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("===== Ollama Completions for SQL Server =====");
                Console.WriteLine("============ Deployment Manager =============");
                Console.WriteLine();
                Console.WriteLine($"Respository root directory: {repoRootDirectory}");
                Console.WriteLine($"         Scripts directory: {scriptsDirectory}");
                Console.WriteLine($"          Images directory: {imagesDirectory}");
                Console.WriteLine($"Database connection string: \"{connectionString}\"");
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
                        command = new CheckExternalServicesCommand(connectionString, null);
                        break;

                    case "2":
                        command = new ListHostedModelsCommand(null, null);
                        break;

                    case "3":
                        command = new EstablishClrDatabaseCommand(connectionString, $"{scriptsDirectory}/establish-clr--database.sql");
                        break;

                    case "4":
                        command = new EstablishEmptyTablesCommand(connectionString, $"{scriptsDirectory}/establish-empty-tables.sql");
                        break;

                    case "5":
                        command = new PopulateConfigAndSchemaCommand(connectionString, $"{scriptsDirectory}/populate-config-and-schema.sql");
                        break;

                    case "6":
                        command = new PopulateDemoDataCommand(connectionString, $"{scriptsDirectory}/populate-demo-data.sql");
                        break;

                    case "7":
                        command = new RelinkClrAssemblyCommand(connectionString, $"{scriptsDirectory}/relink-clr-assembly.sql");
                        break;

                    case "8":
                        command = new CheckThisDeploymentCommand(connectionString, $"{scriptsDirectory}/check-this-deployment.sql");
                        break;

                    case "9":
                        command = new RevertThisDeploymentCommand(connectionString, $"{scriptsDirectory}/revert-this-deployment.sql");
                        break;

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
