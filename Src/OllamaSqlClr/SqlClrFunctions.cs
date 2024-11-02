using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using static OllamaSqlClr.OllamaHelpers;
using JsonClrLibrary;

namespace OllamaSqlClr
{
    public class SqlClrFunctions
    {
        #region "Complete Prompt"

        [SqlFunction(DataAccess = DataAccessKind.None)]
        public static SqlString CompletePrompt(
            SqlString askPrompt,
            SqlString morePrompt)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";

            try
            {
                var result = GetModelResponseToPrompt(prompt, "llama3.2");
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

        [SqlFunction(FillRowMethodName = "FillRow_CompleteMultiplePrompts")]
        public static IEnumerable CompleteMultiplePrompts(
            SqlString askPrompt,
            SqlString morePrompt,
            SqlInt32 numCompletions)
        {
            var prompt = $"{askPrompt.Value} {morePrompt.Value}";
            var completions = new List<(Guid, string)>();
            List<int> context = null;

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    var result = GetModelResponseToPrompt(prompt, "llama3.2", context);

                    string response = JsonSerializerDeserializer.GetStringField(result, "response");

                    // Generate a unique GUID for each completion
                    Guid completionGuid = Guid.NewGuid();
                    completions.Add((completionGuid, response));

                    // Feed the current context into the next request
                    context = JsonSerializerDeserializer.GetIntegerArray(result, "context");
                }

                return completions;
            }
            catch (Exception ex)
            {
                return new List<(Guid, string)> { (Guid.Empty, $"Error: {ex.Message}") };
            }
        }

        // Updated FillRow method to output both a GUID and the completion string
        public static void FillRow_CompleteMultiplePrompts(
            object completionObj,
            out SqlGuid completionGuid,
            out SqlString ollamaCompletion)
        {
            var (guid, completion) = ((Guid, string))completionObj;

            completionGuid = new SqlGuid(guid);
            ollamaCompletion = new SqlString(completion);
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

    } // end class SqlClrFunctions
} // end namespace OllamaSqlClr
