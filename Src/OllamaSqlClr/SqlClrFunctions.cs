using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using static OllamaSqlClr.OllamaHelper;

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
                string result = CallOllamaService(prompt.Value, null);

                string response = ExtractField(result, "response");
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
            int[] contextArray = null; // Placeholder for context tracking between calls

            try
            {
                for (int i = 0; i < numCompletions.Value; i++)
                {
                    string result = CallOllamaService(prompt.Value, contextArray);
                    string response = ExtractField(result, "response");

                    // Generate a unique GUID for each completion
                    Guid completionGuid = Guid.NewGuid();
                    completions.Add((completionGuid, response));

                    // Update context for next iteration if needed
                    contextArray = ExtractContextArray(result);
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

