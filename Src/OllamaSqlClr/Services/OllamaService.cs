﻿using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Collections;

using JsonClrLibrary;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using OllamaSqlClr.DataAccess;
using System.Data;

namespace OllamaSqlClr.Services
{
    public class OllamaService : IOllamaService
    {
        private readonly string _sqlConnection;
        private readonly string _apiUrl;

        private readonly IQueryValidator _queryValidator;
        private readonly IQueryLogger _queryLogger;
        private readonly IOllamaApiClient _apiClient;
        private readonly ISqlCommandHelper _sqlCommandHelper;
        private readonly ISqlQueryHelper _sqlQueryHelper;
        private readonly IDatabaseExecutor _databaseExecutor;

        public OllamaService(
            string sqlConnection,
            string apiUrl,

            IQueryValidator queryValidator = null,
            IQueryLogger queryLogger = null,
            IOllamaApiClient apiClient = null,
            ISqlCommandHelper sqlCommandHelper = null,
            ISqlQueryHelper sqlQueryHelper = null,
            IDatabaseExecutor databaseExecutor = null)
        {
            _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));

            // Initialize helpers
            _databaseExecutor = databaseExecutor ?? new DatabaseExecutor(_sqlConnection);

            _queryValidator = queryValidator ?? new QueryValidator();
            _apiClient = apiClient ?? new OllamaApiClient(_apiUrl);

            _queryLogger = queryLogger ?? new QueryLogger(_databaseExecutor);
            _sqlCommandHelper = sqlCommandHelper ?? new SqlCommandHelper(_databaseExecutor);
            _sqlQueryHelper = sqlQueryHelper ?? new SqlQueryHelper(_databaseExecutor);
        }

        public SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";
            try
            {
                var result = _apiClient.GetModelResponseToPrompt(prompt, modelName.Value);
                string response = JsonHandler.GetStringField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        public IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";
            var completions = new List<CompletionRow>();
            List<int> context = null;

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    var result = _apiClient.GetModelResponseToPrompt(prompt, modelName.Value, context);
                    string response = JsonHandler.GetStringField(result, "response");

                    var completion = new CompletionRow
                    {
                        CompletionGuid = Guid.NewGuid(),
                        ModelName = modelName.Value,
                        OllamaCompletion = response
                    };
                    completions.Add(completion);

                    // Feed the current context into the next request
                    context = JsonHandler.GetIntegerArray(result, "context");
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
                var modelCount = JsonHandler.GetIntegerByPath(result, "models.length");

                for (var i = 0; i < modelCount; i++)
                {
                    var modelInfo = new ModelInformationRow
                    {
                        ModelGuid = Guid.NewGuid(),
                        Name = JsonHandler.GetStringByPath(result, $"models[{i}].name"),
                        Model = JsonHandler.GetStringByPath(result, $"models[{i}].model"),
                        ReferToName = JsonHandler.GetStringByPath(result, $"models[{i}].model").Split(':')[0],
                        ModifiedAt = JsonHandler.GetDateByPath(result, $"models[{i}].modified_at"),
                        Size = JsonHandler.GetLongByPath(result, $"models[{i}].size"),
                        Family = JsonHandler.GetStringByPath(result, $"models[{i}].details.family"),
                        ParameterSize = JsonHandler.GetStringByPath(result, $"models[{i}].details.parameter_size"),
                        QuantizationLevel = JsonHandler.GetStringByPath(result, $"models[{i}].details.quantization_level"),
                        Digest = JsonHandler.GetStringByPath(result, $"models[{i}].digest")
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

        #region "Query from prompt feature"

        public IEnumerable QueryFromPrompt(SqlString modelName, SqlString prompt)
        {
            /*
             * TODO: roadmap for this function:
             *
             *      var schemaJson = GetTableSchema(databaseName)
             *      (isSchemaAccepted, context) = GiveSchemaToModel(modelname, schemaJson)
             *      var proposedQuery = GetProposedQuery(modelName, prompt, context)
             *      var isValidQuery = ValidateProposedQuery(proposedQuery)
             *      var result = RunProposedQuery(proposedQuery)
             *      return result
             */

            var schemaJson = GetTableSchemaJson();

            string proposedQuery = "SELECT * FROM support_emails WHERE sentiment = 'glad'";
            var resultList = new List<QueryFromPromptRow>();

            try
            {
                var isUnsafe = _queryValidator.IsUnsafe(proposedQuery);
                var isNoReply = _queryValidator.IsNoReply(proposedQuery);
                string jsonResult = "";

                if (isUnsafe || isNoReply)
                {
                    jsonResult = "{\"error\": \"Query was unsafe or 'no reply'\"}";
                }
                else
                {
                    string jsonQuery = $"SELECT * FROM ({proposedQuery}) AS Data FOR JSON AUTO";
                    var dataTable = _databaseExecutor.ExecuteQuery(jsonQuery);

                    if (dataTable.Rows.Count > 0)
                    {
                        jsonResult = dataTable.Rows[0][0].ToString();
                    }
                }

                resultList.Add(new QueryFromPromptRow
                {
                    QueryGuid = Guid.NewGuid(),
                    ModelName = modelName.Value,
                    Prompt = prompt.Value,

                    // ProposedQuery = proposedQuery,
                    ProposedQuery = schemaJson,

                    Result = jsonResult,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                resultList.Add(new QueryFromPromptRow
                {
                    QueryGuid = Guid.NewGuid(),
                    ModelName = modelName.Value,
                    Prompt = prompt.Value,

                    // ProposedQuery = proposedQuery,
                    ProposedQuery = schemaJson,

                    Result = $"{{\"error\": \"{ex.Message}\"}}",
                    Timestamp = DateTime.UtcNow
                });
            }

            return resultList;
        }

        private string _cachedSchemaJson;
        private DateTime _lastSchemaUpdate;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10); // Adjust as needed

        private string GetTableSchemaJson()
        {
            // Check if the cache is still valid
            if (_cachedSchemaJson != null && DateTime.UtcNow - _lastSchemaUpdate < _cacheExpiration)
            {
                return _cachedSchemaJson;
            }

            // Query the database for the schema
            string schemaQuery = @"
                SELECT TOP 1 SchemaJson 
                FROM DB_Schema
                ORDER BY ID DESC;
            ";

            DataTable resultTable = _databaseExecutor.ExecuteQuery(schemaQuery);

            if (resultTable.Rows.Count > 0)
            {
                _cachedSchemaJson = resultTable.Rows[0]["SchemaJson"].ToString();
                _lastSchemaUpdate = DateTime.UtcNow; // Update the cache timestamp
                return _cachedSchemaJson;
            }

            // If no schema is found, clear the cache
            _cachedSchemaJson = string.Empty;
            return _cachedSchemaJson;
        }

        #endregion

    }
}
