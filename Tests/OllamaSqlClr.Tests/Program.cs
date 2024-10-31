using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;

namespace OllamaSqlClr.Tests
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Warm up by loading required assemblies
            WarmUp();

            // Define and run tests
            List<Action> tests = new List<Action> {
                TestCompletePrompt,
                TestCompleteMultiplePrompts
            };

            int index = 1;
            foreach (var test in tests)
            {
                try
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($"Test {index}: {test.Method.Name} begins...");
                    test.Invoke();
                    Debug.WriteLine($"{test.Method.Name} PASSED");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{test.Method.Name} failed: {ex.Message}");
                }
                index++;
            }
            Debug.WriteLine("");
        }

        private static void WarmUp()
        {
            Debug.WriteLine("Starting warm-up...");

            try
            {
                // Load frequently used assemblies
                var dataType = typeof(System.Data.DataTable); // Ensures System.Data.dll is loaded
                Assembly.Load("OllamaSqlClr"); // Load assembly under test

                // Add any additional assemblies you expect to use
                Debug.WriteLine("Warm-up completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warm-up failed: {ex.Message}");
            }
        }

        private static void TestCompletePrompt()
        {
            var ask = new SqlStringWrapper("Why is the sky blue?").ToSqlString();
            var addContext = new SqlStringWrapper("Answer in less than twenty words.").ToSqlString();

            var result = SqlClrFunctions.CompletePrompt(ask, addContext);

            Debug.WriteLine("");
            Debug.WriteLine($"CompletePrompt(\"{ask}\", \"{addContext}\"): \n    Completion: {result.Value}");
            Debug.WriteLine("");
        }

        private static void TestCompleteMultiplePrompts()
        {
            var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
            var addContext = new SqlStringWrapper("It must be fruit-bearing. Limit your answer to ten words.").ToSqlString();
            var numCompletions = new SqlInt32(5);

            var results = SqlClrFunctions.CompleteMultiplePrompts(ask, addContext, numCompletions);

            Debug.WriteLine("");
            Debug.WriteLine($"CompleteMultiplePrompt(\"{ask}\", \"{addContext}\", {numCompletions})");
            foreach (var result in results)
            {
                var (completionGuid, ollamaCompletion) = ((Guid, string))result;
                Debug.WriteLine($"    Row: {completionGuid}, {ollamaCompletion}");
            }
            Debug.WriteLine("");
        }
    }

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
    }
}
