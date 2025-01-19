using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace DeploymentManager.Commands
{
    public static class RunScript
    {
        private const int DefaultCommandTimeout = 300; // 5 minutes

        public static void Execute(Dictionary<string, string> settings, string scriptName)
        {
            if (!PromptUserConfirmation(scriptName))
                return;

            var scriptPath = GetScriptPath(settings, scriptName);
            var commands = ParseScriptCommands(scriptPath);

            try
            {
                ExecuteCommands(commands, settings["SqlServerConnection"]);
            }
            catch (SqlException ex)
            {
                LogSqlError(ex);
                throw;
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private static bool PromptUserConfirmation(string scriptName)
        {
            Console.WriteLine();
            Console.Write($"You are about to run SQL script \"{scriptName}\"... Continue?  N // ");
            var response = Console.ReadLine()?.Trim().ToUpper() ?? "N";

            if (response != "Y")
            {
                Console.WriteLine("Operation cancelled.");
                return false;
            }
            return true;
        }

        private static string GetScriptPath(Dictionary<string, string> settings, string scriptName)
        {
            var scriptPath = Path.Combine(settings["ScriptsDirectory"], scriptName);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"The script file \"{scriptPath}\" was not found.");
            }
            return scriptPath;
        }

        private static string[] ParseScriptCommands(string scriptPath)
        {
            var scriptContent = File.ReadAllText(scriptPath);
            return scriptContent.Split(
                new[] { "\r\nGO\r\n", "\nGO\n", "\rGO\r" },
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        private static void ExecuteCommands(string[] commands, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            if (string.IsNullOrWhiteSpace(command)) continue;
                            ExecuteSingleCommand(command, connection, transaction);
                        }

                        transaction.Commit();
                        Console.WriteLine();
                        Console.WriteLine("All commands executed successfully.");
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void ExecuteSingleCommand(string commandText, SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand(commandText, connection, transaction))
            {
                command.CommandTimeout = DefaultCommandTimeout;

                // Handle SQL Server messages (PRINT, RAISERROR, etc.)
                var messageQueue = new Queue<string>();
                connection.InfoMessage += (sender, e) =>
                {
                    messageQueue.Enqueue(e.Message);
                };

                using (var reader = command.ExecuteReader())
                {
                    // Process any messages that came before first result set
                    while (messageQueue.Count > 0)
                    {
                        WriteColoredLine(messageQueue.Dequeue(), ConsoleColor.DarkCyan, "[INFO]: ");
                    }

                    do
                    {
                        // Process any accumulated messages before each result set
                        while (messageQueue.Count > 0)
                        {
                            WriteColoredLine(messageQueue.Dequeue(), ConsoleColor.DarkCyan, "[INFO]: ");
                        }

                        if (reader.FieldCount > 0)
                        {
                            ProcessDataResults(reader);
                        }
                    } while (reader.NextResult());

                    // Process any remaining messages after all result sets
                    while (messageQueue.Count > 0)
                    {
                        WriteColoredLine(messageQueue.Dequeue(), ConsoleColor.DarkCyan, "[INFO]: ");
                    }
                }
            }
        }

        private static void ProcessDataResults(SqlDataReader reader)
        {
            WriteColoredLine("=== Result Set ===", ConsoleColor.Yellow);

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"{reader.GetName(i)}: ");
                    Console.ResetColor();
                    Console.Write($"{reader[i]}  ");
                }
                Console.WriteLine();
            }
        }

        private static void WriteColoredLine(string message, ConsoleColor color, string prefix = "")
        {
            Console.WriteLine();
            Console.ForegroundColor = color;
            Console.WriteLine($"{prefix}{message}");
            Console.ResetColor();
        }

        private static void LogSqlError(SqlException ex)
        {
            Console.WriteLine();
            Console.WriteLine("SQL error occurred while running the script:");
            Console.WriteLine($"Error Number: {ex.Number}");
            Console.WriteLine($"Error Message: {ex.Message}");
            Console.WriteLine($"Line Number: {ex.LineNumber}");
        }

        private static void LogError(Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"An error occurred while running the script: {ex.Message}");
        }
    }
}
