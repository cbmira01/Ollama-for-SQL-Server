using System;
using System.IO;

namespace DeploymentManager.Commands
{
    public class SetUpDatabaseCommand : ICommand
    {
        private readonly string _connectionString;
        private readonly string _scriptPath;

        public SetUpDatabaseCommand(string connectionString, string scriptPath)
        {
            _connectionString = connectionString;
            _scriptPath = scriptPath;
        }

        public void Execute()
        {
            Console.WriteLine("Setting up the database...");

            try
            {
                // Validate the script path
                if (!File.Exists(_scriptPath))
                {
                    Console.WriteLine($"Error: SQL script not found at {_scriptPath}");
                    return;
                }

                // Load and execute the SQL script
                string sqlScript = File.ReadAllText(_scriptPath);
                var dbExecutor = new DatabaseExecutor(_connectionString);
                dbExecutor.ExecuteSql(sqlScript);

                Console.WriteLine("Database setup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while setting up the database: {ex.Message}");
            }
        }
    }
}
