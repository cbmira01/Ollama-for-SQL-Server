using System;
using System.Data.SqlTypes;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SqlClrApiExecutor.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize test result flag
            bool allTestsPassed = true;

            // Test 1: ExecuteApiCommand
            try
            {
                var apiUrl = new SqlStringWrapper("https://httpbin.org/anything").ToSqlString();

                // Build the request body using a C# object
                var requestBodyObject = new { 
                    key1 = "value of the key field",
                    key2 = "another value of a key field"
                };

                // Serialize the object to JSON
                var requestBody = new SqlStringWrapper(JsonConvert.SerializeObject(requestBodyObject)).ToSqlString();

                var result = CommandExecutor.ExecuteApiCommand(apiUrl, requestBody, "full");
                Debug.WriteLine($"Test 1 - ExecuteApiCommand: {result.Value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 1 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            //// Test 2: CompletePrompt
            try
            {
                var apiUrl = new SqlStringWrapper("http://localhost:11434/api/generate").ToSqlString();
                var ask = new SqlStringWrapper("Why is the sky blue?").ToSqlString();
                var body = new SqlStringWrapper("Answer in less than twenty words.").ToSqlString();

                var result = CommandExecutor.CompletePrompt(apiUrl, ask, body);
                Debug.WriteLine($"Test 2 - CompletePrompt: {result.Value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 2 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            //// Test 3: CompleteMultiplePrompts
            try
            {
                var apiUrl = new SqlStringWrapper("http://localhost:11434/api/generate").ToSqlString();
                var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
                var body = new SqlStringWrapper("It must be fruit-bearing.").ToSqlString();
                var numCompletions = new SqlInt32(5); 

                var results = CommandExecutor.CompleteMultiplePrompts(apiUrl, ask, body, numCompletions);

                Debug.WriteLine("Test 3 - CompleteMultiplePrompts:");

                foreach (string result in results)
                {
                    Debug.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 3 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            // Determine if all tests passed and exit with appropriate status code
            if (allTestsPassed)
            {
                Debug.WriteLine("All tests passed successfully.");
                Environment.Exit(0);  // Exit code 0 means success
            }
            else
            {
                Debug.WriteLine("Some tests failed.");
                Environment.Exit(1);  // Exit code 1 indicates failure
            }
        }

    } // end class

    // Wrapper classes for testing purposes
    public class SqlStringWrapper
    {
        public string Value { get; set; }

        public SqlStringWrapper(string value)
        {
            Value = value;
        }

        public SqlString ToSqlString()
        {
            return new SqlString(Value);
        }

        public override string ToString()
        {
            return Value;
        }
    } // end class 
} // end namespace
