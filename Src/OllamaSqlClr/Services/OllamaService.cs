﻿using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;

using OllamaSqlClr.Helpers;
using JsonClrLibrary;
using OllamaSqlClr.Models;

namespace OllamaSqlClr
{
    // Service class that combines all the helpers for SQL CLR functions to use.
    //
    // This class exists to facilitate DI for testing.
    //
    // Fill-Row static classes stay in the SqlClrFunctions class, 
    //      since they are called directly by SQL Server.

    public class OllamaService
    {
        private readonly QueryValidator _validator;
        private readonly QueryLogger _queryLogger;
        private readonly OllamaApiClient _apiClient;
        private readonly SqlCommand _sqlCommand;
        private readonly SqlQuery _sqlQuery;

        public OllamaService(
            QueryValidator validator, 
            QueryLogger queryLogger, 
            OllamaApiClient apiClient, 
            SqlCommand sqlCommand,
            SqlQuery sqlQuery)
        {
            _validator = validator;
            _queryLogger = queryLogger;
            _apiClient = apiClient;
            _sqlCommand = sqlCommand;
            _sqlQuery = sqlQuery;
        }

        public SqlString CompletePrompt(string modelName, string askPrompt, string morePrompt)
        {
            var prompt = $"{askPrompt} {morePrompt}";
            try
            {
                var result = _apiClient.GetModelResponseToPrompt(prompt, modelName);
                string response = JsonSerializerDeserializer.GetStringField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        public IEnumerable CompleteMultiplePrompts(
            SqlString modelName,
            SqlString askPrompt,
            SqlString morePrompt,
            SqlInt32 numCompletions)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";
            var completions = new List<CompletionRow>();
            List<int> context = null;

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    var result = _apiClient.GetModelResponseToPrompt(prompt, modelName.Value, context);
                    string response = JsonSerializerDeserializer.GetStringField(result, "response");

                    var completion = new CompletionRow
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
                var errorCompletion = new CompletionRow
                {
                    CompletionGuid = Guid.Empty,
                    ModelName = modelName.Value,
                    OllamaCompletion = $"Error: {ex.Message}"
                };

                return new List<CompletionRow> { errorCompletion };
            }
        }

        public IEnumerable GetAvailableModels()
        {
            var availableModels = new List<ModelInformationRow>();

            try
            {
                var result = _apiClient.GetOllamaApiTags();
                var modelCount = JsonSerializerDeserializer.GetIntegerByPath(result, "models.length");

                for (var i = 0; i < modelCount; i++)
                {
                    var modelInfo = new ModelInformationRow
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
                return new List<ModelInformationRow> { new ModelInformationRow { ModelGuid = Guid.Empty, Name = $"Error: {ex.Message}" } };
            }
        }

        public SqlString QueryFromPrompt(SqlString modelName, SqlString askPrompt)
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
                
                if (!_validator.IsSafeQuery(proposedQuery))
                {
                    return new SqlString("Error: proposed query had unsafe keywords.");
                }

                string procedureName = _sqlCommand.CreateProcedureFromQuery(proposedQuery);
                DataTable resultTable = _sqlCommand.RunTemporaryProcedure(procedureName);


                (bool isDropped, string message) = _sqlCommand.DropTemporaryProcedure(procedureName);
                if (!isDropped)
                {
                    return new SqlString(message);
                }

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

                    _queryLogger.LogQueryError(prompt, proposedQuery, errorNumber, errorMessage, errorLine);
                    return new SqlString($"Result error {errorNumber}: {errorMessage} at line {errorLine}.");
                }
                else
                {
                    _queryLogger.LogQuerySuccess(prompt, proposedQuery);
                    return new SqlString("Query executed successfully.");
                }
            }
            catch (Exception ex)
            {
                return new SqlString($"Exception error: {ex.Message}");
            }
        }

    }
}