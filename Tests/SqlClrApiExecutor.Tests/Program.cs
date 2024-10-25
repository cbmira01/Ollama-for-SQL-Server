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
                var escapedRequestBody = requestBody.Value.Replace("\"", "\\\""); // Escape double quotes

                var result = CommandExecutor.ExecuteApiCommand(apiUrl, escapedRequestBody, "full");
                Debug.WriteLine($"Test 1 - ExecuteApiCommand: {result.Value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 1 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            //// Test 2: CompletePrompt
            //try
            //{
            //    var apiUrl = new SqlStringWrapper("https://api.example.com");
            //    var promptResult = CommandExecutor.CompletePrompt(apiUrl, new SqlStringWrapper("Hello"), new SqlStringWrapper("World"));
            //    Console.WriteLine($"Test 2 - CompletePrompt: {promptResult.Value}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Test 2 Failed: {ex.Message}");
            //    allTestsPassed = false;
            //}

            //// Test 3: CompleteMultiplePrompts
            //try
            //{
            //    var apiUrl = new SqlStringWrapper("https://api.example.com");
            //    var ask = new SqlStringWrapper("Generate multiple responses");
            //    var body = new SqlStringWrapper("for this test case.");
            //    var numCompletions = 3;

            //    var results = CommandExecutor.CompleteMultiplePrompts(apiUrl, ask, body, numCompletions);

            //    Console.WriteLine("Test 3 - CompleteMultiplePrompts:");
            //    foreach (string result in results)
            //    {
            //        Console.WriteLine(result);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Test 3 Failed: {ex.Message}");
            //    allTestsPassed = false;
            //}

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
