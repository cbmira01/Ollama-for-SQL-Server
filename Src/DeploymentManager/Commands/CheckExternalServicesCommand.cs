﻿using System;
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
                if (IsSqlServerReady(settings["SqlServerConnection"])) 
                {
                    Console.WriteLine($"    SQL Server is ready!");
                }
                else 
                {
                    Console.WriteLine($"    SQL Server is NOT ready.");
                    Console.WriteLine($"    Via the SQL Server Configuration Manager, make sure MSSQLSERVER and its agent are in a running state.");
                }

                Console.WriteLine();

                if (IsOllamaApiServerReady(settings["ApiUrl"]))
                {
                    Console.Write($"    Ollama API server is ready!");
                }
                else
                {
                    Console.WriteLine($"    Ollama API server is NOT ready.");
                    Console.WriteLine($"    Check your MSI or Docker installation of Ollama, make sure it is serving on {settings["ApiUrl"]}");
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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
                    return response.StatusCode == System.Net.HttpStatusCode.OK;
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
