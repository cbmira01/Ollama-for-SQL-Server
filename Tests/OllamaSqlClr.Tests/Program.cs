using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;

namespace OllamaSqlClr.Tests
{
    class Program
    {
        public static void Main(string[] args)
        {
            List<Action> tests = new List<Action> {
                        TestCompletePrompt,
                        TestCompleteMultiplePrompts
                };

            foreach (var test in tests)
            {
                try
                {
                    test.Invoke();
                    Debug.WriteLine($"{test.Method.Name} passed.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{test.Method.Name} failed: {ex.Message}");
                }
            }
        }

        private static void TestCompletePrompt()
        {
            var ask = new SqlStringWrapper("Why is the sky blue?").ToSqlString();
            var additional = new SqlStringWrapper("Answer in less than twenty words.").ToSqlString();

            var result = SqlClrFunctions.CompletePrompt(ask, additional);
            Debug.WriteLine($"Test 1 - CompletePrompt: {result.Value}");
        }

        private static void TestCompleteMultiplePrompts()
        {
            var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
            var additional = new SqlStringWrapper("It must be fruit-bearing. Limit your answer to ten words.").ToSqlString();
            var numCompletions = new SqlInt32(5);

            var results = SqlClrFunctions.CompleteMultiplePrompts(ask, additional, numCompletions);

            foreach (var result in results)
            {
                var (completionGuid, ollamaCompletion) = ((Guid, string))result;
                Debug.WriteLine($"{completionGuid}: {ollamaCompletion}");
            }
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
