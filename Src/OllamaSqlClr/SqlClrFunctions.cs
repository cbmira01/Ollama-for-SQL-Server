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
            SqlString additionalPrompt)
        {
            var prompt = askPrompt + " " + additionalPrompt;

            try
            {
                var result = CallOllamaService(prompt.Value, "llama3.2", null);

                // Extract the response field
                string response = (string)JsonSerializerDeserializer.GetField(result, "response");
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
            SqlString additionalPrompt,
            SqlInt32 numCompletions)
        {
            var prompt = askPrompt + " " + additionalPrompt;
            var completions = new List<(Guid, string)>();
            List<int> context = null;

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    // Call the service and get the result
                    var result = CallOllamaService(prompt.Value, "llama3.2", context);

                    // Extract the response field
                    string response = (string)JsonSerializerDeserializer.GetField(result, "response");

                    // Generate a unique GUID for each completion
                    Guid completionGuid = Guid.NewGuid();
                    completions.Add((completionGuid, response));

                    // Retrieve and convert the context array from List<object> to List<int>
                    var contextList = JsonSerializerDeserializer.GetField(result, "context") as List<object>;
                    context = contextList?.ConvertAll(item => Convert.ToInt32(item));
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
