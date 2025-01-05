using System;
using System.Data.SqlClient;
using System.IO;

/***
 * 
 * This program will load image files into the TEST database Images table.
 * 
 * Only unique filenames are loaded into the table.
 * 
 * Script22 contains an alternative way to load image files.
 * 
 ***/

namespace LoadImageFiles
{
    class Program
    {
        private const string ConnectionString = "Server=localhost;Database=TEST;Integrated Security=SSPI;";

        static void Main(string[] args)
        {
            string repoRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string imagesPath = Path.Combine(repoRoot, "Images");

            try
            {
                if (!Directory.Exists(imagesPath))
                {
                    Console.WriteLine($"Directory not found: {imagesPath}");
                    return;
                }

                string[] imageFiles = Directory.GetFiles(imagesPath, "*.jpg", SearchOption.TopDirectoryOnly);

                if (imageFiles.Length == 0)
                {
                    Console.WriteLine("No JPEG files found in the directory.");
                    return;
                }

                foreach (string filePath in imageFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        byte[] fileData = File.ReadAllBytes(filePath);

                        InsertImage(fileName, fileData);
                        Console.WriteLine($"Successfully inserted: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process file {filePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static void InsertImage(string fileName, byte[] fileData)
        {
            // Enforce unique filenames
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM Images WHERE FileName = @FileName)
                BEGIN
                    INSERT INTO Images (FileName, ImageData) VALUES (@FileName, @ImageData)
                END";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@ImageData", fileData);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
