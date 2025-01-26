using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Configuration;
using DeploymentManager;

namespace DeploymentManager.Commands
{
    public static class LoadImageFiles
    {
        public static void Execute()
        {
            var imagesDirectory = AppConfig.ImagesDirectory;

            Console.WriteLine();
            Console.WriteLine($"Load JPG, PNG and GIF images from {imagesDirectory}");

            if (!UI.PromptUserConfirmation())
            {
                return;
            }

            Console.WriteLine();

            try
            {
                if (!Directory.Exists(imagesDirectory))
                {
                    UI.WriteColoredLine($"Directory not found: {imagesDirectory}",
                        ConsoleColor.Red, newLine: true);

                    return;
                }

                if (!CheckDatabaseAndCreateTable())
                {
                    UI.WriteColoredLine("Creation of the Images table failed. Has the [AI_Lab] database been established?",
                        ConsoleColor.Red, newLine: true);

                    return;
                }

                var extensions = new[] { "*.jpg", "*.png", "*.gif" };
                var imageFiles = extensions
                    .SelectMany(ext => Directory.GetFiles(imagesDirectory, ext, SearchOption.TopDirectoryOnly))
                    .ToArray();

                if (imageFiles.Length == 0)
                {
                    UI.WriteColoredLine($"No image files found in the directory {imagesDirectory}",
                        ConsoleColor.Red, newLine: true);
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
                        UI.WriteColoredLine($"    Successfully inserted: {fileName}", 
                            ConsoleColor.DarkYellow, newLine: true);

                        numSuccess++;
                    }
                    catch (Exception ex)
                    {
                        UI.WriteColoredLine($"Failed to process file {filePath}: {ex.Message}", 
                            ConsoleColor.Red, newLine: true);
                    }
                }

                Console.WriteLine();
                UI.WriteColoredLine($"        {numSuccess} image files successfully loaded.",
                    ConsoleColor.Green, newLine: true);
            }
            catch (Exception ex)
            {
                UI.WriteColoredLine($"An error occurred: {ex.Message}", 
                    ConsoleColor.Red, newLine: true);
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
            using (var connection = new SqlConnection(AppConfig.SqlServerConnection))
            {
                connection.Open();

                using (var command = new SqlCommand(query, connection))
                {
                    configureCommand?.Invoke(command);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
