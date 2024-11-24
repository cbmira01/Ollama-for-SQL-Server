using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Collections;

using JsonClrLibrary;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using OllamaSqlClr.DataAccess;

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

        public SqlString QueryFromPrompt()
        {
            try
            {
                string proposedQuery = "SELECT * FROM support_emails WHERE sentiment = 'glad';";

                var isSafe = _queryValidator.IsSafeQuery(proposedQuery);
                var isNoReply = _queryValidator.IsNoReply(proposedQuery);

                var procedureName = _sqlCommandHelper.CreateProcedureFromQuery(proposedQuery);
                var dataTable = _sqlCommandHelper.RunProcedure(procedureName);
                var result = dataTable.ToString();

                bool isSuccessful = false;
                string message = string.Empty;
                (isSuccessful, message) = _sqlCommandHelper.DropProcedure(procedureName);

                return new SqlString($"Data table: {result}; DROP: {isSuccessful}, {message}");
            }
            catch (Exception ex)
            {
                return new SqlString($"Exception error: {ex.Message}");
            }
        }

    }
}
