using System.Data.SqlTypes;
using System;
using System.Collections.Generic;
using System.Collections;

using JsonClrLibrary;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;
using OllamaSqlClr.DataAccess;
using System.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.Data.SqlClient;

namespace OllamaSqlClr.Services
{
    public class OllamaService : IOllamaService
    {
        #region Service class constructor

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

        #endregion

        #region CompletePrompt feature

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

        #endregion

        #region CompleteMultiplePrompt feature

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

        #endregion

        #region GetAvailableModels feature

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

        #endregion

        #region Query from prompt feature

        public IEnumerable QueryFromPrompt(SqlString modelName, SqlString prompt)
        {
            var resultList = new List<QueryFromPromptRow>();
            string jsonTableResult = "";

            string proposedQuery = AskModelForQuery(modelName.Value, prompt.Value);

            var isUnsafe = _queryValidator.IsUnsafe(proposedQuery);
            var isNoReply = _queryValidator.IsNoReply(proposedQuery);
            var isRejected = _queryValidator.IsRejected(proposedQuery);

            if (isUnsafe || isNoReply || isRejected)
            {
                jsonTableResult = "{\"error\": \"Query was rejected\"}";
                resultList.Add(new QueryFromPromptRow
                {
                    QueryGuid = Guid.NewGuid(),
                    ModelName = modelName.Value,
                    Prompt = prompt.Value,
                    ProposedQuery = proposedQuery,
                    Result = jsonTableResult,
                    Timestamp = DateTime.UtcNow
                });
                return resultList;
            }

            try
            {
                var dataTable = _databaseExecutor.ExecuteQuery(proposedQuery);

                if (dataTable.Rows.Count > 0)
                {
                    jsonTableResult = JsonHandler.DataTableToJson(dataTable);
                }
                else
                {
                    jsonTableResult = "[]"; // Empty JSON array for no results
                }

                resultList.Add(new QueryFromPromptRow
                {
                    QueryGuid = Guid.NewGuid(),
                    ModelName = modelName.Value,
                    Prompt = prompt.Value,
                    ProposedQuery = proposedQuery,
                    Result = jsonTableResult,
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
                    ProposedQuery = proposedQuery,
                    Result = $"{{\"error\": \"{ex.Message}\"}}",
                    Timestamp = DateTime.UtcNow
                });
            }

            return resultList;
        }

        private string AskModelForQuery(string modelName, string prompt)
        {
            var sqlPreamble = GetValueFromKey("sqlPreamble");
            var sqlGuidelines = GetValueFromKey("sqlGuidelines");
            var schemaPreamble = GetValueFromKey("schemaPreamble");
            var schemaJson = GetValueFromKey("schemaJson");
            var sqlPostscript = GetValueFromKey("sqlPostscript");
            var doubleCheck = GetValueFromKey("doubleCheck");

            var complexPrompt = $"{sqlPreamble} {sqlGuidelines} {schemaPreamble} {schemaJson} {sqlPostscript} {prompt}";
            List<int> context = new List<int>();
            string response = "";

            var retryLimit = 3;  // TODO: Configure retries as needed
            for (int attempt = 0; attempt < retryLimit; attempt++) 
            {
                try
                {
                    var result = _apiClient.GetModelResponseToPrompt(complexPrompt, modelName, context);
                    response = _queryValidator.CleanQuery(JsonHandler.GetStringField(result, "response"));
                    context = JsonHandler.GetIntegerArray(result, "context");

                    // Append the attempt number as a comment to the query
                    response += $" -- attempt {attempt + 1}";

                    if (_databaseExecutor.TryQuery(response))
                    {
                        return response;
                    }

                    complexPrompt = $"{doubleCheck} {response}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");
                }
            }

            return "rejected";
        }

        #endregion

        #region Image classification feature

        public SqlString ExamineImage(SqlString modelName, SqlString prompt, SqlBytes imageData)
        {
            if (string.IsNullOrEmpty(modelName.Value))
            {
                throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName));
            }

            if (string.IsNullOrEmpty(prompt.Value))
            {
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
            }

            if (imageData == null || imageData.IsNull)
            {
                throw new ArgumentNullException(nameof(imageData), "Image data cannot be null.");
            }

            // Convert SqlBytes to Base64 string
            byte[] imageBytes = imageData.Value;
            string base64Image = Convert.ToBase64String(imageBytes);

            try
            {
                var result = _apiClient.GetModelResponseToImage(prompt.Value, modelName.Value, base64Image);
                string response = JsonHandler.GetStringField(result, "response");
                return new SqlString(response);
            }
            catch (Exception ex)
            {
                return new SqlString($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Service class private fields and methods

        private Dictionary<string, string> _keyValueCache;
        private DateTime _lastCacheUpdate;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(1); // TODO: Configure as needed

        private void PopulateKeyValueCache()
        {
            // Check if the cache is still valid
            if (_keyValueCache != null && DateTime.UtcNow - _lastCacheUpdate < _cacheExpiration)
            {
                return; // Cache is still valid, no need to repopulate
            }

            // Query the database for all key-value pairs
            string query = @"
                SELECT
                    [Key], [Value]
                FROM KeyValuePairs;
            ";

            DataTable resultTable = _databaseExecutor.ExecuteQuery(query);

            // Rebuild the cache
            _keyValueCache = new Dictionary<string, string>();
            foreach (DataRow row in resultTable.Rows)
            {
                string key = row["Key"].ToString();
                string value = row["Value"].ToString();

                if (!string.IsNullOrEmpty(key)) // Ensure keys are not null or empty
                {
                    _keyValueCache[key] = value;
                }
            }

            _lastCacheUpdate = DateTime.UtcNow; // Update the cache timestamp
        }

        private string GetValueFromKey(string key)
        {
            // Ensure the cache is populated
            PopulateKeyValueCache();

            // Lookup the key in the cache
            return _keyValueCache != null && _keyValueCache.TryGetValue(key, out var value)
                ? value
                : null;
        }

        #endregion
    }
}
