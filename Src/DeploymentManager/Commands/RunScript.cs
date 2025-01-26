using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Configuration;
using DeploymentManager;

namespace DeploymentManager.Commands
{
    public static class RunScript
    {
        // Declare the following symbols available for SQL scripts
        private static List<string> _declareStatements { get; } = new List<string>
        {
            DeclareSymbol("RepoRootDirectory"),
            DeclareSymbol("SanityComment"),
            DeclareSymbol("SanityModelName"),
            DeclareSymbol("SanityPrompt1"),
            DeclareSymbol("SanityPrompt2")
        };

        // Actions and console colors can be controlled by these keywords from SQL PRINT statements
        private static readonly Dictionary<string, ConsoleColor> _keywordColors = new Dictionary<string, ConsoleColor>
        {
            { "[WARNING]", ConsoleColor.Yellow },
            { "[ERROR]", ConsoleColor.Red },
            { "[SYMBOL]", ConsoleColor.DarkYellow },
            { "[CHECK]", ConsoleColor.DarkYellow },
            { "[STEP]", ConsoleColor.DarkCyan }
        };

        // Private list of messages from SQL Server to skip
        private static List<string> _nonHelpfulMessages = new List<string>
        {
            "Run the RECONFIGURE statement to install"
        };

        private static bool _errorDetected = false; // error feedback from running SQL script

        public static void Execute(string scriptName)
        {
            if (!IsReleaseBuildAvailable())
            {
                Console.WriteLine();
                Console.WriteLine("    The RELEASE build is not available. Exiting.");

                return;
            }

            Console.WriteLine();
            Console.WriteLine($"You are about to run SQL script \"{scriptName}\"");

            if (!UI.PromptUserConfirmation())
            {
                return;
            }

            _errorDetected = false;

            var scriptPath = GetScriptPath(scriptName);
            var commands = ParseScriptCommands(scriptPath);

            try
            {
                ExecuteCommands(commands);
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

        private static bool IsReleaseBuildAvailable()
        {
            string releaseArtifactPath = 
                Path.Combine(
                    AppConfig.RepoRootDirectory, 
                    "Src",
                    "OllamaSqlClr",
                    "bin", 
                    "Release", 
                    "OllamaSqlClr.dll");

            if (File.Exists(releaseArtifactPath))
            {
                return true;
            }

            Console.WriteLine();
            UI.WriteColoredLine(
                $"    Release build artifact not found at: {releaseArtifactPath}", 
                ConsoleColor.Red, newLine:true);

            return false;
        }


        private static string GetScriptPath(string sn)
        {
            var scriptPath = Path.Combine(AppConfig.ScriptsDirectory, sn);
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

        private static void ExecuteCommands(string[] commands)
        {
            using (var connection = new SqlConnection(AppConfig.SqlServerConnection))
            {
                connection.Open();

                try
                {
                    var declareBlock = string.Join(Environment.NewLine, _declareStatements);
                    commands[0] = declareBlock + Environment.NewLine + commands[0];

                    foreach (var command in commands)
                    {
                        if (string.IsNullOrWhiteSpace(command)) continue;

                        // Stop processing if an error was detected
                        if (_errorDetected)
                        {
                            Console.WriteLine();
                            UI.WriteColoredLine(
                                "Skipping remaining commands because of a previous error", 
                                ConsoleColor.Yellow, newLine:true);

                            return;
                        }

                        ExecuteSingleCommand(command, connection);
                    }

                    if (!_errorDetected)
                    {
                        Console.WriteLine();
                        UI.WriteColoredLine("All commands executed successfully.",
                            ConsoleColor.Green, newLine: true);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        private static string DeclareSymbol(string symbolName)
        {
            var value = AppConfig.GetSymbolValue(symbolName);

            var declarePart = $"DECLARE @{symbolName} NVARCHAR(MAX) = '{value}';";
            var printPart = $"PRINT '[SYMBOL]: Supplied by Deployment Manager: {symbolName} = ' + @{symbolName}";
            var declaration = $"{declarePart}\n{printPart}\n";

            return declaration;
        }

        private static void ExecuteSingleCommand(string commandText, SqlConnection connection)
        {
            using (var command = new SqlCommand(commandText, connection))
            {
                command.CommandTimeout = AppConfig.SqlCommandTimeoutSecs;

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
            while (mQ.Count > 0)
            {
                var message = mQ.Dequeue();

                // Filter out non-helpful messages
                if (_nonHelpfulMessages.Any(skipText => message.Contains(skipText)))
                {
                    continue;
                }

                // Determine if the message contains any of the defined keywords
                var matchedKeyword = _keywordColors.Keys.FirstOrDefault(k => message.Contains(k));

                if (matchedKeyword != null)
                {
                    // Assign a color based on the keyword matched in the SQL PRINT
                    var color = _keywordColors[matchedKeyword];
                    var prefix = "";

                    // Set error flag if it's an error message
                    if (matchedKeyword == "[ERROR]")
                    {
                        _errorDetected = true;
                    }

                    Console.WriteLine();
                    UI.WriteColoredLine(message, color, prefix, newLine:true);
                }
                else
                {
                    // Default case for informational messages
                    Console.WriteLine();
                    UI.WriteColoredLine(message, ConsoleColor.DarkGray, "[INFO]: ", newLine:true);
                }
            }
        }

        private static void ProcessDataResults(SqlDataReader reader)
        {
            Console.WriteLine();
            UI.WriteColoredLine("=== Result Set ===", ConsoleColor.Yellow, newLine: true);

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    UI.WriteColoredLine($"{reader.GetName(i)}: ", ConsoleColor.Green, newLine:false);
                    UI.WriteColoredLine($"{reader[i]}  ", ConsoleColor.White, newLine: false);
                }
                Console.WriteLine();
            }
        }

        private static void LogSqlError(SqlException ex)
        {
            Console.WriteLine();
            UI.WriteColoredLine("An SQL error occurred while running the script:", ConsoleColor.Red, newLine: true);
            Console.WriteLine($"    Error Number: {ex.Number}");
            Console.WriteLine($"    Error Message: {ex.Message}");
            Console.WriteLine($"    Line Number: {ex.LineNumber}");
        }

        private static void LogError(Exception ex)
        {
            Console.WriteLine();
            UI.WriteColoredLine($"An error occurred while running the script: {ex.Message}", ConsoleColor.Red, newLine: true);
        }
    }
}
