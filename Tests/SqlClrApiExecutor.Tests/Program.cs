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

            //// Test 1: CompletePrompt
            try
            {
                var ask = new SqlStringWrapper("Why is the sky blue?").ToSqlString();
                var additional = new SqlStringWrapper("Answer in less than twenty words.").ToSqlString();

                var result = ApiExecutor.CompletePrompt(ask, additional);
                Debug.WriteLine($"Test 1 - CompletePrompt: {result.Value}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 1 Failed: {ex.Message}");
                allTestsPassed = false;
            }

            //// Test 2: CompleteMultiplePrompts
            try
            {
                var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
                var additional = new SqlStringWrapper("It must be fruit-bearing. Limit your answer to ten words.").ToSqlString();
                var numCompletions = new SqlInt32(5); 

                var results = ApiExecutor.CompleteMultiplePrompts(ask, additional, numCompletions);

                Debug.WriteLine("Test 2 - CompleteMultiplePrompts:");

                foreach (string result in results)
                {
                    Debug.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Test 2 Failed: {ex.Message}");
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
