using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

using JsonClrLibrary;
using static OllamaSqlClr.OllamaHelpers;
using OllamaSqlClr.DataAccess;
using System.CodeDom.Compiler;

namespace OllamaSqlClr
{
    public class SqlClrFunctions
    {
        #region "CompletePrompt"

        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(
            SqlString modelName,
            SqlString askPrompt,
            SqlString morePrompt)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";

            try
            {
                var result = GetModelResponseToPrompt(prompt, modelName.Value);
                string response = JsonSerializerDeserializer.GetStringField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        #endregion

        #region "CompleteMultiplePrompts"

        public class CompletionInfo
        {
            public Guid CompletionGuid { get; set; }
            public string ModelName { get; set; } = string.Empty;
            public string OllamaCompletion { get; set; } = string.Empty;
        }

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(
            SqlString modelName,
            SqlString askPrompt,
            SqlString morePrompt,
            SqlInt32 numCompletions)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";
            var completions = new List<CompletionInfo>();
            List<int> context = null;

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    var result = GetModelResponseToPrompt(prompt, modelName.Value, context);
                    string response = JsonSerializerDeserializer.GetStringField(result, "response");

                    var completion = new CompletionInfo
                    {
                        CompletionGuid = Guid.NewGuid(),
                        ModelName = modelName.Value,
                        OllamaCompletion = response
                    };
                    completions.Add(completion);

                    // Feed the current context into the next request
                    context = JsonSerializerDeserializer.GetIntegerArray(result, "context");
                }

                return completions;
            }
            catch (Exception ex)
            {
                var errorCompletion = new CompletionInfo
                {
                    CompletionGuid = Guid.Empty,
                    ModelName = modelName.Value,
                    OllamaCompletion = $"Error: {ex.Message}"
                };

                return new List<CompletionInfo> { errorCompletion };
            }
        }

        // Updated FillRow method to output both a GUID and the completion string
        public static void FillRow_CompleteMultiplePrompts(
            object completionObj,
            out SqlGuid completionGuid,
            out SqlString modelName,
            out SqlString ollamaCompletion)
        {
            var completionInfo = (CompletionInfo)completionObj;

            completionGuid = new SqlGuid(completionInfo.CompletionGuid);
            modelName = new SqlString(completionInfo.ModelName);
            ollamaCompletion = new SqlString(completionInfo.OllamaCompletion);
        }

        #endregion

        #region "GetAvailableModels"

        public class ModelInfo
        {
            public Guid ModelGuid { get; set; }
            public string Name { get; set; }
            public string Model { get; set; }
            public string ReferToName { get; set; }
            public DateTime ModifiedAt { get; set; }
            public long Size { get; set; }
            public string Family { get; set; }
            public string ParameterSize { get; set; }
            public string QuantizationLevel { get; set; }
            public string Digest { get; set; }
        }

        [SqlFunction(FillRowMethodName = "FillRow_GetAvailableModels")]
        public static IEnumerable GetAvailableModels()
        {
            var availableModels = new List<ModelInfo>();

            try
            {
                var result = GetOllamaApiTags();
                var modelCount = JsonSerializerDeserializer.GetIntegerByPath(result, "models.length");

                for (var i = 0; i < modelCount; i++)
                {
                    var modelInfo = new ModelInfo
                    {
                        ModelGuid = Guid.NewGuid(),
                        Name = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].name"),
                        Model = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].model"),
                        ReferToName = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].model").Split(':')[0],
                        ModifiedAt = JsonSerializerDeserializer.GetDateByPath(result, $"models[{i}].modified_at"),
                        Size = JsonSerializerDeserializer.GetLongByPath(result, $"models[{i}].size"),
                        Family = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].details.family"),
                        ParameterSize = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].details.parameter_size"),
                        QuantizationLevel = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].details.quantization_level"),
                        Digest = JsonSerializerDeserializer.GetStringByPath(result, $"models[{i}].digest")
                    };

                    availableModels.Add(modelInfo);
                }

                return availableModels;
            }
            catch (Exception ex)
            {
                return new List<ModelInfo> { new ModelInfo { ModelGuid = Guid.Empty, Name = $"Error: {ex.Message}" } };
            }
        }

        public static void FillRow_GetAvailableModels(
            object modelObj,
            out SqlGuid modelGuid,
            out SqlString name,
            out SqlString model,
            out SqlString referToName,
            out SqlDateTime modifiedAt,
            out SqlInt64 size,
            out SqlString family,
            out SqlString parameterSize,
            out SqlString quantizationLevel,
            out SqlString digest)
        {
            var modelInfo = (ModelInfo)modelObj;

            modelGuid = new SqlGuid(modelInfo.ModelGuid);
            name = new SqlString(modelInfo.Name);
            model = new SqlString(modelInfo.Model);
            referToName = new SqlString(modelInfo.ReferToName);
            modifiedAt = new SqlDateTime(modelInfo.ModifiedAt);
            size = new SqlInt64(modelInfo.Size);
            family = new SqlString(modelInfo.Family);
            parameterSize = new SqlString(modelInfo.ParameterSize);
            quantizationLevel = new SqlString(modelInfo.QuantizationLevel);
            digest = new SqlString(modelInfo.Digest);
        }

        #endregion

        #region "QueryFromPrompt"

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString QueryFromPrompt(SqlString modelName, SqlString askPrompt, IDatabaseExecutor dbExecutor)
        {
            var leadingPrompt = "Write only the SQL code, with no additional commentary, for the following query in double quotes:";
            var trailingPrompt = "Your response should contain either SQL syntax only, or the words 'no reply'.";
            var framePrompt = "Do not frame your reply in any sort of code block or quotes, since only a bare reply is wanted.";
            var codePrompt = "Do not use any character code points, encodings, or entities in your response; these are unwanted.";

            var prompt = $"{leadingPrompt} \"{askPrompt.Value}\" {trailingPrompt} {framePrompt} {codePrompt}";

            try
            {
                // TODO: Example API call result (replace with actual API call)
                string proposedQuery = "SELECT * FROM support_emails WHERE sentiment = 'glad';";

                if (!IsSafe(proposedQuery))
                {
                    return new SqlString("Error: proposed query had unsafe keywords.");
                }

                DataTable resultTable = BuildAndRunTempProcedure(proposedQuery, dbExecutor);

                if (resultTable.Columns.Contains("ErrorNumber"))
                {
                    string errorNumber = resultTable.Rows[0]["ErrorNumber"].ToString();
                    string errorMessage = resultTable.Rows[0]["ErrorMessage"].ToString();
                    string errorLine = resultTable.Rows[0]["ErrorLine"].ToString();

                    return new SqlString("Error: the query produced errors.");
                }

                // Check for errors in the result table
                if (resultTable.Columns.Contains("ErrorNumber"))
                {
                    string errorNumber = resultTable.Rows[0]["ErrorNumber"].ToString();
                    string errorMessage = resultTable.Rows[0]["ErrorMessage"].ToString();
                    string errorLine = resultTable.Rows[0]["ErrorLine"].ToString();

                    LogThisQuery(prompt, proposedQuery, errorNumber, errorMessage, errorLine, dbExecutor);
                    return $"Result error {errorNumber}: {errorMessage} at line {errorLine}.";
                }
                else
                {
                    LogThisQuery(prompt, proposedQuery, null, null, null, dbExecutor);
                    return "Query executed successfully.";
                }
            }
            catch (Exception ex)
            {
                return new SqlString($"Exception error: {ex.Message}");
            }
        }

        private static bool IsSafe(string query)
        {
            string unsafeKeywordsPattern = @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|GRANT|REVOKE|DENY)\b|no reply";
            return !Regex.IsMatch(query, unsafeKeywordsPattern, RegexOptions.IgnoreCase);
        }

        private static DataTable BuildAndRunTempProcedure(string query, IDatabaseExecutor dbExecutor)
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

        private static void LogThisQuery(
            string prompt, 
            string proposedQuery, 
            string errorNumber, 
            string errorMessage, 
            string errorLine,
            IDatabaseExecutor dbExecutor)
        {
            // Log the prompt and query
            string logQueryCommand = $@"
                USE [TEST];
                GO

                INSERT INTO QueryPromptLog (Prompt, ProposedQuery, ErrorNumber, ErrorMessage, ErrorLine) 
                    VALUES ({prompt}, {proposedQuery}, {errorNumber}, {errorMessage}, {errorLine})
                GO";

            dbExecutor.ExecuteNonQuery(logQueryCommand);

            return;
        }

        #endregion

    } // end class SqlClrFunctions
} // end namespace OllamaSqlClr
