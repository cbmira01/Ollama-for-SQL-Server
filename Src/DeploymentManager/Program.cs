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
                Console.WriteLine("===== Deployment Manager =====");
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Set up database");
                Console.WriteLine("2. Set up CLR assemblies");
                Console.WriteLine("3. Create tables");
                Console.WriteLine("4. Load data");
                Console.WriteLine("5. Run Experiment 1");
                Console.WriteLine("6. Run Experiment 2");
                Console.WriteLine("0. Exit");
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
                        Console.WriteLine("Invalid choice. Press any key to try again...");
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
