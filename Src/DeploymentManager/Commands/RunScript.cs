using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DeploymentManager.Commands
{
    public static class RunScript
    {
        private const int DefaultCommandTimeout = 300; // 5 minutes
        private static Dictionary<string, string> _settings;
        private static string _scriptName;

        public static void Execute(Dictionary<string, string> settings, string scriptName)
        {
            if (!PromptUserConfirmation(scriptName))
                return;

            _settings = settings;
            _scriptName = scriptName;

            var scriptPath = GetScriptPath();
            var commands = ParseScriptCommands(scriptPath);

            try
            {
                ExecuteCommands(settings, commands);
            }
            catch (SqlException ex)
            {
                LogSqlError(ex);
                return;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return;
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

        private static string GetScriptPath()
        {
            var scriptPath = Path.Combine(_settings["ScriptsDirectory"], _scriptName);
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

        private static void ExecuteCommands(Dictionary<string, string> settings, string[] commands)
        {
            var connectionString = settings["SqlServerConnection"];

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {

                    var declareStatements = new List<string>();
                    declareStatements.Add(DeclarationTemplate("RepoRootDirectory"));

                    var declareBlock = string.Join(Environment.NewLine, declareStatements);
                    commands[0] = declareBlock + Environment.NewLine + commands[0];

                    foreach (var command in commands)
                    {
                        if (string.IsNullOrWhiteSpace(command)) continue;
                        ExecuteSingleCommand(command, connection);
                    }

                    Console.WriteLine();
                    Console.WriteLine("All commands executed successfully.");
                }
                catch
                {
                    throw;
                }
            }
        }

        private static string DeclarationTemplate(string symbol)
        {
            var value = _settings[symbol];

            var declarePart = $"DECLARE @{symbol} NVARCHAR(MAX) = '{value}';";
            var printPart = $"PRINT '[SYMBOL]: {symbol} = ' + @{symbol}";
            var declaration = $"{declarePart}\n{printPart}\n";

            return declaration;
        }

        private static void ExecuteSingleCommand(string commandText, SqlConnection connection)
        {
            using (var command = new SqlCommand(commandText, connection))
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
                    do
                    {
                        // Process any accumulated messages before each result set
                        ProcessMessageQueue(messageQueue);

                        if (reader.FieldCount > 0)
                        {
                            ProcessDataResults(reader);
                        }
                    } while (reader.NextResult());

                    // Process any remaining messages after all result sets
                    ProcessMessageQueue(messageQueue);
                }
            }
        }

        private static void ProcessMessageQueue(Queue<string> mQ)
        {
            var keywords = new List<string> {
                    "[SYMBOL]",
                    "[CHECK]",
                    "[STEP]",
                    "[ERROR]"
                };

            while (mQ.Count > 0)
            {
                var prefix = "[INFO]: ";
                var message = mQ.Dequeue();

                // Filter some non-helpful things out of the message stream
                if (message.Contains("Run the RECONFIGURE statement to install")) continue;

                // Flag certain script keywords
                if (keywords.Any(k => message.Contains(k)))
                {
                    prefix = "";
                }

                WriteColoredLine(message, ConsoleColor.DarkCyan, prefix);
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
