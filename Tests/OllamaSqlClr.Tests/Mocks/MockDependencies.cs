using OllamaSqlClr.DataAccess;
using OllamaSqlClr.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OllamaSqlClr.Tests.Mocks
{
    public class MockDatabaseExecutor : IDatabaseExecutor
    {
        public DataTable ExecuteQuery(string query)
        {
            // Return a mock DataTable
            var table = new DataTable();
            table.Columns.Add("Column1");
            table.Rows.Add("Mocked Row 1");
            table.Rows.Add("Mocked Row 2");
            return table;
        }

        public void ExecuteNonQuery(string commandText)
        {
            // No-op for testing
        }

        public SqlConnection GetConnection()
        {
            return null; // No connection needed in test context
        }
    }

    public class MockQueryLogger : IQueryLogger
    {
        private readonly IDatabaseExecutor _dbExecutor;

        public MockQueryLogger(IDatabaseExecutor dbExecutor)
        {
            _dbExecutor = dbExecutor;
        }

        public void LogQuerySuccess(string prompt, string query) 
        {
            LogQueryExecution(prompt, query, null, null, null);
        }

        public void LogQueryError(string prompt, string query, string errorNumber, string errorMessage, string errorLine) 
        {
            LogQueryExecution(prompt, query, "Error number", "Error Message", "Error Line");
        }

        public void LogQueryExecution(string prompt, string query, string errorNumber, string errorMessage, string errorLine) 
        {
            string logQueryCommand = $@"
                INSERT INTO QueryPromptLog (Prompt, Query, ErrorNumber, ErrorMessage, ErrorLine)
                VALUES ('{prompt}', '{query}', '{errorNumber}', '{errorMessage}', '{errorLine}');";

            // Use the injected database executor to "execute" the SQL
            _dbExecutor.ExecuteNonQuery(logQueryCommand);
        }
    }

    public class MockOllamaApiClient : IOllamaApiClient
    {
        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName)
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("response", $"Mocked response for model '{modelName}' and prompt '{prompt}'"),
                new KeyValuePair<string, object>("modelName", modelName),
                new KeyValuePair<string, object>("prompt", prompt)
            };
        }

        public List<KeyValuePair<string, object>> GetModelResponseToPrompt(string prompt, string modelName, List<int> context)
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("response", $"Mocked response with context for model '{modelName}' and prompt '{prompt}'"),
                new KeyValuePair<string, object>("modelName", modelName),
                new KeyValuePair<string, object>("prompt", prompt),
                new KeyValuePair<string, object>("context", context != null ? string.Join(",", context) : "null")
            };
        }

        public List<KeyValuePair<string, object>> GetOllamaApiTags()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", "mockModel"),
                new KeyValuePair<string, object>("model", "mockModel:v1"),
                new KeyValuePair<string, object>("modified_at", "2024-01-01T00:00:00Z"),
                new KeyValuePair<string, object>("size", 1024),
                new KeyValuePair<string, object>("family", "mock"),
                new KeyValuePair<string, object>("parameter_size", "large"),
                new KeyValuePair<string, object>("quantization_level", "high"),
                new KeyValuePair<string, object>("digest", "abcd1234")
            };
        }
    }
}

