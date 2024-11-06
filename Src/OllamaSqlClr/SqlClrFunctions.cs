using System.Collections;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using OllamaSqlClr.Models;

namespace OllamaSqlClr
{
    public static class SqlClrFunctions
    {
        private static readonly OllamaService _ollamaService = new OllamaService(
            new QueryValidator(),
            new QueryLogger(new DatabaseExecutor()),
            new OllamaApiClient("http://127.0.0.1:11434"),
            new SqlCommand(new DatabaseExecutor()),
            new SqlQuery(new DatabaseExecutor()));

        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt)
        {
            return _ollamaService.CompletePrompt(modelName.Value, askPrompt.Value, morePrompt.Value);
        }

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(SqlString modelName, SqlString askPrompt, SqlString morePrompt, SqlInt32 numCompletions)
        {
            return _ollamaService.CompleteMultiplePrompts(
                modelName.Value,
                askPrompt.Value,
                morePrompt.Value,
                numCompletions);
        }

        [SqlFunction(FillRowMethodName = "FillRow_GetAvailableModels")]
        public static IEnumerable GetAvailableModels()
        {
            return _ollamaService.GetAvailableModels();
        }

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

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString QueryFromPrompt(SqlString modelName, SqlString askPrompt)
        {
            return _ollamaService.QueryFromPrompt(
                modelName.Value,
                askPrompt.Value);
        }

        // TODO: QueryFromPrompt FillRow goes here

    } // end class SqlClrFunctions
} // end namespace OllamaSqlClr
