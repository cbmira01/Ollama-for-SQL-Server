using JsonClrLibrary;
using OllamaSqlClr.DataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace OllamaSqlClr
{
    public static class OllamaHelpers
    {
        public static string ApiGenerateUrl { get; set; } = "http://127.0.0.1:11434/api/generate";
        public static string ApiTagsUrl { get; set; } = "http://127.0.0.1:11434/api/tags";
        public static int RequestTimeout { get; set; } = 100000; // Default timeout of 100 seconds

        public static List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt,
            string modelName)
        {
            return GetModelResponseToPrompt(prompt, modelName, null);
        }

        public static List<KeyValuePair<string, object>> GetModelResponseToPrompt(
            string prompt, 
            string modelName, 
            List<int> context)
        {
            var data = new List<KeyValuePair<string, object>>
            {
                JsonBuilder.CreateField("model", modelName),
                JsonBuilder.CreateField("prompt", prompt),
                JsonBuilder.CreateField("stream", false),
                JsonBuilder.CreateArray("context", context)
            };

            string json = JsonSerializerDeserializer.Serialize(data);

#if DEBUG
            Console.WriteLine("Request...");
            JsonSerializerDeserializer.DumpJson(json);
#endif

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiGenerateUrl);
            request.Timeout = RequestTimeout;
            request.Method = "POST";
            request.ContentType = "application/json";

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(json);
            }

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }

#if DEBUG
            Console.WriteLine("Response...");
            JsonSerializerDeserializer.DumpJson(responseJson);
#endif

            return JsonSerializerDeserializer.Deserialize(responseJson);
        }

        public static List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ApiTagsUrl);
            request.Timeout = RequestTimeout;
            request.Method = "GET";

            string responseJson = "";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }

#if DEBUG
            Console.WriteLine("Response...");
            JsonSerializerDeserializer.DumpJson(responseJson);
#endif

            return JsonSerializerDeserializer.Deserialize(responseJson);
        }

        public static bool IsSafe(string query)
        {
            string unsafeKeywordsPattern = @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|GRANT|REVOKE|DENY)\b|no reply";
            return !Regex.IsMatch(query, unsafeKeywordsPattern, RegexOptions.IgnoreCase);
        }

        public static DataTable BuildAndRunTempProcedure(string query, IDatabaseExecutor dbExecutor)
        {
            string limitedQuery = $@"
                SELECT TOP 100 * FROM ({query}) AS LimitedResult";

            string wrappedQuery = $@"
                BEGIN TRY
                    {limitedQuery}
                END TRY
                BEGIN CATCH
                    SELECT 
                        ERROR_NUMBER() AS ErrorNumber,
                        ERROR_MESSAGE() AS ErrorMessage,
                        ERROR_LINE() AS ErrorLine;
                END CATCH";

            string procedureName = "#TempProc_" + Guid.NewGuid().ToString("N");

            string createProcedureCommand = $@"
                CREATE PROCEDURE {procedureName}
                AS
                BEGIN
                    SET NOCOUNT ON;
                    {wrappedQuery}
                END";

            // Create the temporary procedure
            dbExecutor.ExecuteNonQuery(createProcedureCommand);

            // Execute the procedure and retrieve results
            DataTable resultTable = new DataTable();
            string executeProcedureCommand = $"EXEC {procedureName};";

            using (var cmd = new SqlCommand(executeProcedureCommand, dbExecutor.GetConnection()))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(resultTable);
            }

            // Drop the temporary procedure
            string dropProcedureCommand = $"DROP PROCEDURE {procedureName}";
            dbExecutor.ExecuteNonQuery(dropProcedureCommand);

            return resultTable;
        }

        public static void LogThisQuery(
            string prompt,
            string proposedQuery,
            string errorNumber,
            string errorMessage,
            string errorLine,
            IDatabaseExecutor dbExecutor)
        {
            string logQueryCommand = @"
                INSERT INTO QueryPromptLog (Prompt, ProposedQuery, ErrorNumber, ErrorMessage, ErrorLine) 
                VALUES (@Prompt, @ProposedQuery, @ErrorNumber, @ErrorMessage, @ErrorLine);";

            using (var cmd = new SqlCommand(logQueryCommand, dbExecutor.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@Prompt", (object)prompt ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProposedQuery", (object)proposedQuery ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorNumber", (object)errorNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorMessage", (object)errorMessage ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ErrorLine", (object)errorLine ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

    } // end class OllamaHelpers
} // end namespace OllamaSqlClr
