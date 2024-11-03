﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using static OllamaSqlClr.SqlClrFunctions;

namespace OllamaSqlClr.Tests
{
    class Program
    {

        #region "Test Harness"

        public static void Main(string[] args)
        {
            // Warm up by loading required assemblies
            WarmUp();

            // Define and run tests
            List<Action> tests = new List<Action> {
                TestCompletePrompt,
                TestCompleteMultiplePrompts,
                TestGetAvailableModels,
                TestQueryFromPrompt
            };

            int index = 1;
            foreach (var test in tests)
            {
                try
                {
                    Debug.WriteLine("");
                    Debug.WriteLine($"Test {index}: {test.Method.Name} begins...");
                    test.Invoke();
                    Debug.WriteLine($"Test {index}: {test.Method.Name} PASSED");
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
                Assembly.Load("JsonClrLibrary"); // Load JSON support library

                // Add any additional assemblies you expect to use
                Debug.WriteLine("Warm-up completed successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warm-up failed: {ex.Message}");
            }
        }

        #endregion

        private static void TestCompletePrompt()
        {
            var modelName = new SqlStringWrapper("llama3.2").ToSqlString();
            var ask = new SqlStringWrapper("Why is the sky blue?").ToSqlString();
            var addContext = new SqlStringWrapper("Answer in less than twenty words.").ToSqlString();

            var result = SqlClrFunctions.CompletePrompt(modelName, ask, addContext);

            Debug.WriteLine("");
            Debug.WriteLine($"CompletePrompt(\"{modelName}\", \"{ask}\", \"{addContext}\"): \n    Completion: {result.Value}");
            Debug.WriteLine("");
        }

        private static void TestCompleteMultiplePrompts()
        {
            var modelName = new SqlStringWrapper("llama3.2").ToSqlString();
            var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
            var addContext = new SqlStringWrapper("It must be fruit-bearing. Limit your answer to ten words.").ToSqlString();
            var numCompletions = new SqlInt32(5);

            var results = SqlClrFunctions.CompleteMultiplePrompts(modelName, ask, addContext, numCompletions);

            Debug.WriteLine("");
            Debug.WriteLine($"CompleteMultiplePrompt(\"{modelName}\", \"{ask}\", \"{addContext}\", {numCompletions})");
            foreach (var result in results)
            {
                var completionInfo = (CompletionInfo)result;

                Debug.WriteLine($"    Row: {completionInfo.CompletionGuid}, {completionInfo.ModelName}, {completionInfo.OllamaCompletion}");
            }
            Debug.WriteLine("");
        }

        private static void TestGetAvailableModels()
        {
            var results = SqlClrFunctions.GetAvailableModels();

            Debug.WriteLine("");

            foreach (var result in results)
            {
                var modelInfo = (ModelInfo)result;

                Debug.WriteLine("    Row:");
                Debug.WriteLine($"        ModelGuid: {modelInfo.ModelGuid}");
                Debug.WriteLine($"        Name: {modelInfo.Name}");
                Debug.WriteLine($"        Model: {modelInfo.Model}");
                Debug.WriteLine($"        ReferToName: {modelInfo.ReferToName}");
                Debug.WriteLine($"        ModifiedAt: {modelInfo.ModifiedAt}");
                Debug.WriteLine($"        Size: {modelInfo.Size}");
                Debug.WriteLine($"        Family: {modelInfo.Family}");
                Debug.WriteLine($"        ParameterSize: {modelInfo.ParameterSize}");
                Debug.WriteLine($"        QuantizationLevel: {modelInfo.QuantizationLevel}");
                Debug.WriteLine($"        Digest: {modelInfo.Digest}");
            }

            Debug.WriteLine("");
        }

        private static void TestQueryFromPrompt()
        {
            var modelName = new SqlStringWrapper("mistral").ToSqlString();
            var askStrings = new List<string>
            {
                "Find all entries in support_emails where sentiment is glad.",
                "List the top 10 products by sales volume.",
                "12345",
                "What are the recent transactions over $1000?",
                "asdfghjkl"
            };

            foreach (var askText in askStrings)
            {
                var ask = new SqlStringWrapper(askText).ToSqlString();

                var result = SqlClrFunctions.QueryFromPrompt(modelName, ask);

                Debug.WriteLine("");
                Debug.WriteLine($"QueryFromPrompt(\"{modelName.Value}\", \"{ask.Value}\"): \n    Completion: {result.Value}");
                Debug.WriteLine("");
            }
        }

    } // end class Program

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

} // end namespace OllamaSqlClr.Tests
