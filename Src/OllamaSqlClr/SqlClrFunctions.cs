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
        private static IOllamaService _ollamaServiceInstance = new OllamaService();

        public static void SetServiceInstance(IOllamaService service)
        {
            _ollamaServiceInstance = service;
        }

        #region Implemented SQL/CLR functions

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString CompletePrompt(SqlString modelName, SqlString askPrompt, SqlString morePrompt)
        {
            try
            {
                string result = _ollamaServiceInstance.CompletePrompt(modelName.Value, askPrompt.Value, morePrompt.Value);
                return new SqlString(result);
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
                return _ollamaServiceInstance.CompleteMultiplePrompts(modelName.Value, askPrompt.Value, morePrompt.Value, numCompletions.Value);
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
                return _ollamaServiceInstance.GetAvailableModels();
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
        public static IEnumerable QueryFromPrompt(SqlString modelName, SqlString prompt)
        {
            try
            {
                return _ollamaServiceInstance.QueryFromPrompt(modelName.Value, prompt.Value);
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
                return _ollamaServiceInstance.ExamineImage(modelName.Value, prompt.Value, imageData.Value);
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
