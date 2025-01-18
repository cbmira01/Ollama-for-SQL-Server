using System;
using System.Collections.Generic;

namespace DeploymentManager.Commands
{
    public static class CheckExternalServicesCommand
    {
        public static void Execute(Dictionary<string, string> settings)
        {
            Console.WriteLine();
            Console.WriteLine("Checking external services...");
            Console.WriteLine();

            try
            {
                if (IsSqlServerReady()) 
                {
                    Console.WriteLine($"    SQL Server is ready!");
                }
                else 
                {
                    Console.WriteLine($"    SQL Server is NOT ready.");
                    Console.WriteLine($"    Via the SQL Server Configuration Manager, make sure MSSQLSERVER and its agent are in a running state.");
                }
                Console.WriteLine();

                if (IsOllamaApiServerReady())
                {
                    Console.Write($"    Ollama API server is ready!");
                }
                else
                {
                    Console.WriteLine($"    Ollama API server is NOT ready.");
                    Console.WriteLine($"    Check your MSI or Docker installation of Ollama, ensure it is serving on {settings["ApiUrl"]}");
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static bool IsSqlServerReady()
        {
            return true;   // TODO: finish this test
        }

        private static bool IsOllamaApiServerReady() 
        {
            return true;   // TODO: finish this test
        }
    }
}
