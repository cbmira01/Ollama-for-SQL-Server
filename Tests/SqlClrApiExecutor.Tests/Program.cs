using System;
using SqlClrApiExecutor;

namespace SqlClrApiExecutor.Tests
{
    // Wrapper classes for testing purposes
    public class SqlStringWrapper
    {
        public string Value { get; set; }

        public SqlStringWrapper(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Initialize test result flag
            bool allTestsPassed = true;

            // Test 1: ExecuteApiCommand
            try
            {
                var apiUrl = new SqlStringWrapper("https://api.example.com");
                var requestBody = new SqlStringWrapper("{\"key\":\"value\"}");

                var result = CommandExecutor.ExecuteApiCommand(apiUrl, requestBody);
                Console.WriteLine($"Test 1 - ExecuteApiCommand: {result.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test 1 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            // Test 2: CompletePrompt
            try
            {
                var apiUrl = new SqlStringWrapper("https://api.example.com");
                var promptResult = CommandExecutor.CompletePrompt(apiUrl, new SqlStringWrapper("Hello"), new SqlStringWrapper("World"));
                Console.WriteLine($"Test 2 - CompletePrompt: {promptResult.Value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test 2 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            // Test 3: CompleteMultiplePrompts
            try
            {
                var apiUrl = new SqlStringWrapper("https://api.example.com");
                var ask = new SqlStringWrapper("Generate multiple responses");
                var body = new SqlStringWrapper("for this test case.");
                var numCompletions = 3;

                var results = CommandExecutor.CompleteMultiplePrompts(apiUrl, ask, body, numCompletions);

                Console.WriteLine("Test 3 - CompleteMultiplePrompts:");
                foreach (string result in results)
                {
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test 3 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            // Determine if all tests passed and exit with appropriate status code
            if (allTestsPassed)
            {
                Console.WriteLine("All tests passed successfully.");
                Environment.Exit(0);  // Exit code 0 means success
            }
            else
            {
                Console.WriteLine("Some tests failed.");
                Environment.Exit(1);  // Exit code 1 indicates failure
            }
        }

    } // end class
} // end namespace
