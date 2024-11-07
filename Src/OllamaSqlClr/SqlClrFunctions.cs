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
        // Use a factory method to allow mocking
        public static Func<IOllamaService> OllamaServiceFactory { get; set; } = () => new OllamaService(
            new QueryValidator(),
            new QueryLogger(new DatabaseExecutor()),
            new OllamaApiClient("http://127.0.0.1:11434"),
            new SqlCommand(new DatabaseExecutor()),
            new SqlQuery(new DatabaseExecutor())
        );

        public static IOllamaService OllamaServiceInstance => OllamaServiceFactory();

        #region "Implemented SQL/CLR functions"

        [SqlFunction(DataAccess = DataAccessKind.None)]
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
            return OllamaServiceInstance.QueryFromPrompt(modelName, askPrompt);
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

        // TODO: QueryFromPrompt FillRow goes here

        #endregion

    }
}
