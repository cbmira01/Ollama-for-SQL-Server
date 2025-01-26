using System;
using Configuration;

namespace DeploymentManager.Commands
{
    public static class CheckExternalServices
    {
        public static void Execute()
        {
            Console.WriteLine();
            Console.WriteLine("Checking external services...");
            Console.WriteLine();

            try
            {
                if (IsSqlServerReady(AppConfig.SqlServerConnection)) 
                {
                    UI.WriteColoredLine("    SQL Server is ready!", ConsoleColor.Green, newLine: true);
                }
                else 
                {
                    UI.WriteColoredLine("    SQL Server is NOT ready.", ConsoleColor.Red, newLine: true);
                    Console.WriteLine($"        Ensure MSSQLSERVER and its agent are in a running state (via SQL Server Configuration Manager).");
                }

                Console.WriteLine();

                if (IsOllamaApiServerReady(AppConfig.ApiUrl))
                {
                    UI.WriteColoredLine("    Ollama API server is ready!", ConsoleColor.Green, newLine: true);
                }
                else
                {
                    UI.WriteColoredLine("    Ollama API server is NOT ready.", ConsoleColor.Red, newLine: true);
                    Console.WriteLine($"        Check your installation of Ollama, ensure it is serving on {AppConfig.ApiUrl}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                UI.WriteColoredLine($"An error occurred: {ex.Message}", ConsoleColor.Red, newLine: true);
            }
        }

        private static bool IsSqlServerReady(string connectionString)
        {
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                }
                return true; // SQL Server is available
            }
            catch (Exception ex)
            {
                var _ = ex;
                return false;
            }
        }

        private static bool IsOllamaApiServerReady(string apiUrl)
        {
            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(apiUrl);
                request.Method = "GET";
                request.Timeout = 5000; // 5 seconds timeout

                using (var response = (System.Net.HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == System.Net.HttpStatusCode.OK; // Ollama is available
                }
            }
            catch (Exception ex)
            {
                var _ = ex;
                return false;
            }
        }
    }
}
