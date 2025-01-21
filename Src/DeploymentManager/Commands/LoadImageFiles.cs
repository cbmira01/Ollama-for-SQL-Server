using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DeploymentManager.Commands
{
    public static class LoadImageFiles
    {
        private static Dictionary<string, string> _settings;

        public static void Execute(Dictionary<string, string> settings)
        {
            _settings = settings;
            var imagesDirectory = _settings["ImagesDirectory"];

            Console.WriteLine();
            Console.WriteLine($"    Load JPG, PNG and GIF images from {imagesDirectory}");

            if (!PromptUserConfirmation())
                return;

            Console.WriteLine();

            try
            {
                if (!Directory.Exists(imagesDirectory))
                {
                    WriteLineInColor($"Directory not found: {imagesDirectory}", ConsoleColor.Red);
                    return;
                }

                if (!CheckDatabaseAndCreateTable())
                {
                    WriteLineInColor("Creation of the Images table failed. Has the [AI_Lab] database been established?", ConsoleColor.Red);
                    return;
                }

                var extensions = new[] { "*.jpg", "*.png", "*.gif" };
                var imageFiles = extensions
                    .SelectMany(ext => Directory.GetFiles(imagesDirectory, ext, SearchOption.TopDirectoryOnly))
                    .ToArray();

                if (imageFiles.Length == 0)
                {
                    WriteLineInColor($"No image files found in the directory {imagesDirectory}", ConsoleColor.Red);
                    return;
                }

                var numSuccess = 0;
                foreach (string filePath in imageFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        byte[] fileData = File.ReadAllBytes(filePath);

                        InsertImage(fileName, fileData);
                        WriteLineInColor($"    Successfully inserted: {fileName}", ConsoleColor.DarkYellow);

                        numSuccess++;
                    }
                    catch (Exception ex)
                    {
                        WriteLineInColor($"Failed to process file {filePath}: {ex.Message}", ConsoleColor.Red);
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"        {numSuccess} image files successfully loaded.");
            }
            catch (Exception ex)
            {
                WriteLineInColor($"An error occurred: {ex.Message}", ConsoleColor.Red);
            }
        }

        private static bool CheckDatabaseAndCreateTable()
        {
            const string query = @"
                IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'AI_Lab')
                BEGIN
                    RAISERROR('Database [AI_Lab] does not exist; it must be established.', 16, 1);
                    RETURN;
                END

                USE [AI_Lab];

                IF OBJECT_ID('dbo.Images', 'U') IS NOT NULL
                BEGIN
                    DROP TABLE dbo.Images;
                END

                CREATE TABLE dbo.Images (
                    Id INT PRIMARY KEY IDENTITY,
                    FileName NVARCHAR(255) NOT NULL,
                    ImageData VARBINARY(MAX) NOT NULL
                );
            ";

            try
            {
                ExecuteNonQuery(query);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void InsertImage(string fileName, byte[] fileData)
        {
            // Enforce unique filenames
            const string query = @"
                USE [AI_Lab];

                IF NOT EXISTS (SELECT 1 FROM Images WHERE FileName = @FileName)
                BEGIN
                    INSERT INTO Images (FileName, ImageData) VALUES (@FileName, @ImageData)
                END";

            ExecuteNonQuery(query, cmd =>
            {
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@ImageData", fileData);
            });
        }

        private static void ExecuteNonQuery(string query, Action<SqlCommand> configureCommand = null)
        {
            using (var connection = new SqlConnection(_settings["SqlServerConnection"]))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    configureCommand?.Invoke(command);
                    command.ExecuteNonQuery();
                }
            }
        }

        private static bool PromptUserConfirmation()
        {
            Console.Write($"        Continue?  N // ");
            var response = Console.ReadLine()?.Trim().ToUpper() ?? "N";

            if (response != "Y")
            {
                Console.WriteLine("Operation cancelled.");
                return false;
            }
            return true;
        }

        private static void WriteLineInColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

    }
}
