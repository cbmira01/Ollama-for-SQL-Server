using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using static OllamaSqlClr.OllamaHelper;
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
        public static void FillRow_CompleteMultiplePrompts(object completionObj,
            out SqlGuid completionGuid,
            out SqlString ollamaCompletion)
        {
            var (guid, completion) = ((Guid, string))completionObj;

            completionGuid = new SqlGuid(guid);
            ollamaCompletion = new SqlString(completion);
        }

        #endregion

    } // end class SqlClrFunctions
} // end namespace OllamaSqlClr
