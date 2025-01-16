using System;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using OllamaSqlClr.Services;
using OllamaSqlClr.Models;

namespace OllamaSqlClr
{
    public static class SqlClrFunctions
    {
        #region Construct Ollama service instance 

        private static string _sqlConnection;
        private static string _apiUrl;

        // Lazy initialization for the OllamaService instance
        private static Lazy<IOllamaService> _ollamaServiceInstanceLazy = CreateLazyInstance();

        // Public property to access the lazy-initialized instance
        public static IOllamaService OllamaServiceInstance => _ollamaServiceInstanceLazy.Value;

        private static readonly object _configLock = new object();

        static SqlClrFunctions()
        {
            // Default configuration for local setup
            Configure("context connection=true", "http://127.0.0.1:11434");
        }

        public static void Configure(string sqlConnection, string apiUrl)
        {
            lock (_configLock)
            {
                _sqlConnection = sqlConnection ?? throw new ArgumentNullException(nameof(sqlConnection));
                _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            }
        }

        public static void SetMockOllamaServiceInstance(IOllamaService mockService)
        {
            // Allow overriding the lazy instance for tests
            if (mockService == null)
            {
                throw new ArgumentNullException(nameof(mockService));
            }
            _ollamaServiceInstanceLazy = new Lazy<IOllamaService>(() => mockService);
        }


        // Use during unit testing
        public static void ResetOllamaServiceConfiguration()
        {
            // Reset lazy instance to force re-initialization
            _ollamaServiceInstanceLazy = CreateLazyInstance();
        }

        // Creates a new Lazy instance
        private static Lazy<IOllamaService> CreateLazyInstance()
        {
            return new Lazy<IOllamaService>(() =>
            {
                if (string.IsNullOrEmpty(_sqlConnection) || string.IsNullOrEmpty(_apiUrl))
                {
                    throw new InvalidOperationException("OllamaServiceInstance cannot be initialized without SQL Connection and API URL.");
                }

                // Create a new OllamaService with the current configuration
                return new OllamaService(_sqlConnection, _apiUrl);
            });
        }

        #endregion

        #region Implemented SQL/CLR functions

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt)
        {
            try
            {
                return OllamaServiceInstance.CompletePrompt(modelName, askPrompt, morePrompt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error in CompletePrompt: ", ex);
            }
        }

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions)
        {
            try
            {
                return OllamaServiceInstance.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error in CompleteMultiplePrompts: ", ex);
            }
        }

        [SqlFunction(FillRowMethodName = "FillRow_GetAvailableModels")]
        public static IEnumerable GetAvailableModels()
        {
            try
            {
                return OllamaServiceInstance.GetAvailableModels();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error in GetAvailableModels: ", ex);
            }
        }

        [SqlFunction(
            TableDefinition = "QueryGuid UNIQUEIDENTIFIER, ModelName NVARCHAR(MAX), ProposedQuery NVARCHAR(Max), Prompt NVARCHAR(MAX), Result NVARCHAR(MAX), Timestamp DATETIME",
            FillRowMethodName = "FillRow_QueryFromPrompt",
            DataAccess = DataAccessKind.Read
         )]
        public static IEnumerable QueryFromPrompt(SqlString modelname, SqlString prompt)
        {
            try
            {
                return OllamaServiceInstance.QueryFromPrompt(modelname, prompt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error in QueryFromPrompt: ", ex);
            }
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString ExamineImage(SqlString modelName, SqlString prompt, SqlBytes imageData)
        {
            try
            {
                return OllamaServiceInstance.ExamineImage(modelName, prompt, imageData);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error in ExamineImage: ", ex);
            }
        }

        #endregion 

        #region FillRow methods

        public static void FillRow_CompleteMultiplePrompts(
            object completionObj,
            out SqlGuid completionGuid,
            out SqlString modelName,
            out SqlString ollamaCompletion)
        {
            var completionInfo = (CompletionRow)completionObj;

            completionGuid = new SqlGuid(completionInfo.CompletionGuid);
            modelName = new SqlString(completionInfo.ModelName);
            ollamaCompletion = new SqlString(completionInfo.OllamaCompletion);
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
            var modelInfo = (ModelInformationRow)modelObj;

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

        public static void FillRow_QueryFromPrompt(
            object obj,
            out SqlGuid queryGuid,
            out SqlString modelName,
            out SqlString prompt,
            out SqlString proposedQuery,
            out SqlString result,
            out SqlDateTime timestamp)
        {
            var data = (QueryFromPromptRow)obj;

            queryGuid = new SqlGuid(data.QueryGuid);
            modelName = new SqlString(data.ModelName);
            prompt = new SqlString(data.Prompt);
            proposedQuery = new SqlString(data.ProposedQuery);
            result = new SqlString(data.Result);
            timestamp = new SqlDateTime(data.Timestamp);
        }

        #endregion

    }
}
