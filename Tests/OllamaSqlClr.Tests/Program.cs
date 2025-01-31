﻿using OllamaSqlClr.Services;
using OllamaSqlClr.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;

namespace OllamaSqlClr.Tests
{
    class Program
    {

        #region "Test Harness and Warmup"

        public static void Main(string[] args)
        {
            // Warm up by loading required assemblies
            WarmUp();

            // Define and run tests
            List<Action> tests = new List<Action> {
                TestCompletePrompt,
                TestCompleteMultiplePrompts,
                TestGetAvailableModels,
                //TestQueryFromPrompt
            };

            bool allPassed = true;
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
                    allPassed = false;
                }
                index++;
            }
            Debug.WriteLine("");

            if (allPassed)
            {
                Debug.WriteLine("All tests PASSED!");
            }
            else
            {
                Debug.WriteLine("Some tests failed...");
            }

            Debug.WriteLine("");
        }

        private static void WarmUp()
        {
            Debug.WriteLine("");
            Debug.WriteLine("Starting warm-up...");

            try
            {
                // Load frequently used assemblies
                var dataType = typeof(System.Data.DataTable); // Ensures System.Data.dll is loaded
                Assembly.Load("OllamaSqlClr"); // Load assembly under test
                Assembly.Load("JsonClrLibrary"); // Load JSON support library

                // Mock dependencies
                var mockApiClient = new MockOllamaApiClient();
                var mockDatabaseExecutor = new MockDatabaseExecutor();
                var mockQueryLogger = new MockQueryLogger(mockDatabaseExecutor);
                var mockQueryValidator = new MockQueryValidator();

                // Create a mocked OllamaService
                var mockService = new OllamaService(
                    queryLogger: mockQueryLogger,
                    queryValidator: mockQueryValidator,
                    apiClient: mockApiClient,
                    databaseExecutor: mockDatabaseExecutor);

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

            Console.WriteLine("");
            Console.WriteLine($"CompletePrompt(\"{modelName}\", \"{ask}\", \"{addContext}\"): \n    Completion: {result.Value}");
            Console.WriteLine("");
        }

        private static void TestCompleteMultiplePrompts()
        {
            var modelName = new SqlStringWrapper("llama3.2").ToSqlString();
            var ask = new SqlStringWrapper("Tell me the name of a plant.").ToSqlString();
            var addContext = new SqlStringWrapper("It must be fruit-bearing. Limit your answer to ten words.").ToSqlString();
            var numCompletions = new SqlInt32(5);

            var results = SqlClrFunctions.CompleteMultiplePrompts(modelName, ask, addContext, numCompletions);

            Console.WriteLine("");
            Console.WriteLine($"CompleteMultiplePrompt(\"{modelName}\", \"{ask}\", \"{addContext}\", {numCompletions})");
            foreach (var result in results)
            {
                var completionInfo = (Models.CompletionRow)result;

                Console.WriteLine($"    Row: {completionInfo.CompletionGuid}, {completionInfo.ModelName}, {completionInfo.OllamaCompletion}");
            }
            Console.WriteLine("");
        }

        private static void TestGetAvailableModels()
        {
            var results = SqlClrFunctions.GetAvailableModels();

            Console.WriteLine("");

            foreach (var result in results)
            {
                var modelInfo = (Models.ModelInformationRow)result;

                Console.WriteLine("    Row:");
                Debug.WriteLine($"        ModelGuid: {modelInfo.ModelGuid}");
                Console.WriteLine($"        Name: {modelInfo.Name}");
                Console.WriteLine($"        Model: {modelInfo.Model}");
                Console.WriteLine($"        ReferToName: {modelInfo.ReferToName}");
                Debug.WriteLine($"        ModifiedAt: {modelInfo.ModifiedAt}");
                Debug.WriteLine($"        Size: {modelInfo.Size}");
                Debug.WriteLine($"        Family: {modelInfo.Family}");
                Debug.WriteLine($"        ParameterSize: {modelInfo.ParameterSize}");
                Debug.WriteLine($"        QuantizationLevel: {modelInfo.QuantizationLevel}");
                Debug.WriteLine($"        Digest: {modelInfo.Digest}");
            }

            Debug.WriteLine("");
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

}
