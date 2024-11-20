using System;
using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using OllamaSqlClr.Services;
using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;

namespace OllamaSqlClr
{
    public static class SqlClrFunctions
    {
        private static IOllamaService _ollamaServiceInstance;

        // Configuration fields
        private static string _sqlConnection;
        private static string _apiUrl;

        // Method to set configurations
        public static void Configure(string sqlConnection, string apiUrl)
        {
            _sqlConnection = sqlConnection;
            _apiUrl = apiUrl;
        }

        // Property for lazy initialization or external injection
        public static IOllamaService OllamaServiceInstance
        {
            get
            {
                if (_ollamaServiceInstance == null)
                {
                    if (string.IsNullOrEmpty(_sqlConnection) || string.IsNullOrEmpty(_apiUrl))
                    {
                        throw new InvalidOperationException("OllamaServiceInstance cannot be initialized without SQL Connection and API URL.");
                    }

                    // Lazy initialization with configuration
                    _ollamaServiceInstance = new OllamaService(_sqlConnection, _apiUrl);
                }
                return _ollamaServiceInstance;
            }
            set
            {
                // Allows external injection (e.g., for testing)
                _ollamaServiceInstance = value;
            }
        }

        #region "Implemented SQL/CLR functions"

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt)
        {
            return OllamaServiceInstance.CompletePrompt(modelName, askPrompt, morePrompt);
        }

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions)
        {
            return OllamaServiceInstance.CompleteMultiplePrompts(modelName, askPrompt, morePrompt, numCompletions);
        }

        [SqlFunction(FillRowMethodName = "FillRow_GetAvailableModels")]
        public static IEnumerable GetAvailableModels()
        {
            return OllamaServiceInstance.GetAvailableModels();
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString QueryFromPrompt(SqlString modelName, SqlString askPrompt)
        {
            return OllamaServiceInstance.QueryFromPrompt();
        }

        #endregion 

        #region "FillRow methods, do not unit test"

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

        // TODO: QueryFromPrompt FillRow goes here, JSON document table
        //      rows of: GUID, Timestamp, Prompt, ProposedQuery

        #endregion

    }
}
