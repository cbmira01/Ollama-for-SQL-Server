using System;
using System.IO;

namespace DeploymentManager.Commands
{
    public class RevertThisDeploymentCommand : ICommand
    {
        private readonly string _connectionString;
        private readonly string _scriptPath;

        public RevertThisDeploymentCommand(string connectionString, string scriptPath)
        {
            _connectionString = connectionString;
            _scriptPath = scriptPath;
        }

        public void Execute()
        {
            Console.WriteLine("STARTING TO DO SOMETHING...");

            try
            {
                // Validate the script path
                //if (!File.Exists(_scriptPath))
                //{
                //    Console.WriteLine($"Error: SQL script not found at {_scriptPath}");
                //    return;
                //}

                // Load and execute the SQL script
                //string sqlScript = File.ReadAllText(_scriptPath);
                //var dbExecutor = new DatabaseExecutor(_connectionString);
                //dbExecutor.ExecuteSql(sqlScript);

                Console.WriteLine("SUCCESS MESSAGE");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while DOING SOMETHING: {ex.Message}");
            }
        }
    }
}
