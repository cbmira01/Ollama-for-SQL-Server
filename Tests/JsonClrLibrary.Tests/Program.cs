﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace JsonClrLibrary.Tests
{
    internal class Program
    {
        public class JsonTestFramework
        {
            #region "Test harness"

            public class DeserializationMismatchException : Exception
            {
                public DeserializationMismatchException(string message) : base(message) { }
            }

            public static void Main(string[] args)
            {
                // Warm up by loading required assemblies
                WarmUp();

                List<Action> tests = new List<Action>
                {
                        TestSimpleSerialization,
                        TestNestedSerialization,
                        TestDateRecognition,
                        TestOneLevelDuplicateTags,
                        TestNestedDuplicateTags,
                        TestOllamaRequestSerialization,
                        TestOllamaResponseDeserialization,
                        TestOllamaTagDeserialization,
                        TestOllamaSimpleFieldExtraction,
                        TestOllamaNestedFieldExtraction,
                        TestOllamaModelInformationExtraction,
                        TestDeepCompare,
                };

                bool allPassed = true;
                int index = 1;
                foreach (var test in tests)
                {
                    try
                    {
                        Debug.WriteLine("");
                        Debug.WriteLine($"    Test {index}: {test.Method.Name} begins...");
                        test.Invoke();
                        Debug.WriteLine($"    Test {index}: {test.Method.Name} PASSED");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"    Test {index}: {test.Method.Name} failed: {ex.Message}");
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
                Debug.WriteLine("    Starting warm-up...");

                try
                {
                    // Load frequently used assemblies
                    Assembly.Load("JsonClrLibrary"); // Load JSON support library

                    // Add any additional assemblies you expect to use
                    Debug.WriteLine("    Warm-up completed successfully.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"    Warm-up failed: {ex.Message}");
                }
            }

            #endregion

            private static void TestSimpleSerialization()
            {
                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("Name", "Test"),
                    JsonBuilder.CreateField("IsActive", true),
                    JsonBuilder.CreateField("Count", 10)
                );

                string json = JsonHandler.Serialize(data);

                // Expected: {"Name":"Test","IsActive":true,"Count":10}
                if (json != "{\"Name\":\"Test\",\"IsActive\":true,\"Count\":10}")
                {
                    throw new Exception("Simple serialization test failed.");
                }
            }

            private static void TestNestedSerialization()
            {
                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateObject("Parent",
                        JsonBuilder.CreateField("Child", "Value")
                    )
                );

                string json = JsonHandler.Serialize(data);

                // Expected: {"Parent":{"Child":"Value"}}
                if (json != "{\"Parent\":{\"Child\":\"Value\"}}")
                {
                    throw new Exception("Nested serialization test failed.");
                }
            }

            private static void TestDateRecognition()
            {
                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("Timestamp", new DateTime(2024, 10, 27, 12, 0, 0))
                );

                string json = JsonHandler.Serialize(data);

                // Expected: {"Timestamp":"2024-10-27T12:00:00.0000000"}
                if (json != "{\"Timestamp\":\"2024-10-27T12:00:00.0000000\"}")
                {
                    throw new Exception("Date recognition test failed.");
                }
            }

            private static void TestOneLevelDuplicateTags()
            {
                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("model", "nameofmodel"),
                    JsonBuilder.CreateField("prompt", "Why is the sky blue?"),
                    JsonBuilder.CreateField("stream", false),
                    JsonBuilder.CreateField("model", "duplicateModel") // Intentional duplicate key
                );

                try
                {
                    string json = JsonHandler.Serialize(data);
                    throw new Exception("Test One-Level Duplicate Tags failed.");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine("Test One-Level Duplicate Tags passed: " + ex.Message);
                }
            }

            private static void TestNestedDuplicateTags()
            {
                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("model", "nameofmodel"),
                    JsonBuilder.CreateField("prompt", "Why is the sky blue?"),
                    JsonBuilder.CreateObject(
                        "nestedObject",
                        JsonBuilder.CreateField("nestedKey1", "value1"),
                        JsonBuilder.CreateField("nestedKey1", "duplicateValue") // Intentional duplicate key within nested object
                    )
                );

                try
                {
                    string json = JsonHandler.Serialize(data);
                    throw new Exception("Test Nested Duplicate Tags failed.");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine("Test Nested Duplicate Tags passed: " + ex.Message);
                }
            }

            private static void TestOllamaRequestSerialization()
            {
                List<int> context1 = new List<int>() { 1, 2, 3 };
                List<int> context4 = null;

                var data = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("model", "nameofmodel"),
                    JsonBuilder.CreateField("prompt", "Why is the sky blue?"),
                    JsonBuilder.CreateField("stream", false),
                    JsonBuilder.CreateArray("context1", context1),
                    JsonBuilder.CreateArray("context2", "one", "two", "three"),
                    JsonBuilder.CreateArray("context3", true, false, false),
                    JsonBuilder.CreateArray("context4", context4)
                );

                string json = JsonHandler.Serialize(data);

                string shouldBe = "{\"model\":\"nameofmodel\",\"prompt\":\"Why is the sky blue?\",\"stream\":false,\"context1\":[1,2,3],\"context2\":[\"one\",\"two\",\"three\"],\"context3\":[true,false,false],\"context4\":[]}";

                if (json != shouldBe)
                {
                    throw new Exception("Test of Ollama Request Serialization failed.");
                }
            }

            private static void TestOllamaResponseDeserialization()
            {
                string json = "{\"model\": \"zephyr\", \"created_at\": \"2024-10-29T19:43:45.7743388Z\", \"response\": \"The grass appears green...\", \"done\": true, \"done_reason\": \"stop\", \"context\": [28705, 13, 28789, 28766, 1838, 28766, 28767, 13, 7638, 349, 272, 10109, 5344, 28804, 26307, 1215, 15643, 28723, 13, 2, 28705, 13, 28789, 28766, 489, 11143, 28766, 28767, 13, 1014, 10109, 8045, 5344, 1096, 378, 24345, 680, 302, 272, 5344, 275, 26795, 28713, 302, 2061, 325, 28781, 28782, 28734, 28733, 28782, 28787, 28734, 23693, 300, 2612, 28731, 477, 272, 4376, 821, 378, 10612, 1816, 28723, 851, 20757, 349, 2651, 390, 484, 5638, 3126, 19530, 28725, 264, 18958, 466, 1419, 297, 9923, 369, 18156, 706, 298, 6603, 22950, 778, 3408, 1059, 8886, 28724, 448, 21537, 28723], \"total_duration\": 21070342000, \"load_duration\": 7924431300, \"prompt_eval_count\": 30, \"prompt_eval_duration\": 2666814000, \"eval_count\": 67, \"eval_duration\": 10474129000}";
                var data = JsonHandler.Deserialize(json);

                var shouldBe = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("model", "zephyr"),
                    JsonBuilder.CreateField("created_at", DateTime.Parse("2024-10-29T19:43:45.7743388Z")),
                    JsonBuilder.CreateField("response", "The grass appears green..."),
                    JsonBuilder.CreateField("done", true),
                    JsonBuilder.CreateField("done_reason", "stop"),
                    JsonBuilder.CreateArray("context",
                        28705, 13, 28789, 28766, 1838, 28766, 28767, 13, 7638, 349, 272, 10109, 5344, 28804, 26307, 1215,
                        15643, 28723, 13, 2, 28705, 13, 28789, 28766, 489, 11143, 28766, 28767, 13, 1014, 10109, 8045,
                        5344, 1096, 378, 24345, 680, 302, 272, 5344, 275, 26795, 28713, 302, 2061, 325, 28781, 28782,
                        28734, 28733, 28782, 28787, 28734, 23693, 300, 2612, 28731, 477, 272, 4376, 821, 378, 10612,
                        1816, 28723, 851, 20757, 349, 2651, 390, 484, 5638, 3126, 19530, 28725, 264, 18958, 466, 1419,
                        297, 9923, 369, 18156, 706, 298, 6603, 22950, 778, 3408, 1059, 8886, 28724, 448, 21537, 28723
                    ),
                    JsonBuilder.CreateField("total_duration", 21070342000),
                    JsonBuilder.CreateField("load_duration", 7924431300),
                    JsonBuilder.CreateField("prompt_eval_count", 30),
                    JsonBuilder.CreateField("prompt_eval_duration", 2666814000),
                    JsonBuilder.CreateField("eval_count", 67),
                    JsonBuilder.CreateField("eval_duration", 10474129000)
                );

                if (!JsonTestHelpers.DeepCompare(data, shouldBe, out string difference))
                {
                    throw new DeserializationMismatchException($"Test of Ollama Response Deserialization failed. difference: {difference}");
                }
            }

            private static void TestOllamaTagDeserialization()
            {
                string json = "{\"models\":[{\"name\":\"zephyr:latest\",\"model\":\"zephyr:latest\",\"modified_at\":\"2024-10-27T11:51:03.5321962-04:00\",\"size\":4109854934,\"digest\":\"bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"}},{\"name\":\"llama3.2:latest\",\"model\":\"llama3.2:latest\",\"modified_at\":\"2024-09-30T10:37:15.6276545-04:00\",\"size\":2019393189,\"digest\":\"a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"3.2B\",\"quantization_level\":\"Q4_K_M\"}}]}";
                var data = JsonHandler.Deserialize(json);

                var shouldBe = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("models", JsonBuilder.CreateArray(
                        // First model object
                        JsonBuilder.CreateAnonymousObject(
                            JsonBuilder.CreateField("name", "zephyr:latest"),
                            JsonBuilder.CreateField("model", "zephyr:latest"),
                            JsonBuilder.CreateField("modified_at", DateTime.Parse("2024-10-27T11:51:03.5321962-04:00")),
                            JsonBuilder.CreateField("size", 4109854934L),
                            JsonBuilder.CreateField("digest", "bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354"),
                            JsonBuilder.CreateObject("details",
                                JsonBuilder.CreateField("parent_model", ""),
                                JsonBuilder.CreateField("format", "gguf"),
                                JsonBuilder.CreateField("family", "llama"),
                                JsonBuilder.CreateArray("families", "llama"),
                                JsonBuilder.CreateField("parameter_size", "7B"),
                                JsonBuilder.CreateField("quantization_level", "Q4_0")
                            )
                        ),
                        // Second model object
                        JsonBuilder.CreateAnonymousObject(
                            JsonBuilder.CreateField("name", "llama3.2:latest"),
                            JsonBuilder.CreateField("model", "llama3.2:latest"),
                            JsonBuilder.CreateField("modified_at", DateTime.Parse("2024-09-30T10:37:15.6276545-04:00")),
                            JsonBuilder.CreateField("size", 2019393189L),
                            JsonBuilder.CreateField("digest", "a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72"),
                            JsonBuilder.CreateObject("details",
                                JsonBuilder.CreateField("parent_model", ""),
                                JsonBuilder.CreateField("format", "gguf"),
                                JsonBuilder.CreateField("family", "llama"),
                                JsonBuilder.CreateArray("families", "llama"),
                                JsonBuilder.CreateField("parameter_size", "3.2B"),
                                JsonBuilder.CreateField("quantization_level", "Q4_K_M")
                            )
                        )
                    ))
                );

                if (!JsonTestHelpers.DeepCompare(data, shouldBe, out string difference))
                {
                    throw new DeserializationMismatchException($"Test Ollama Tag Deserialization failed. difference: {difference}");
                }
            }

            private static void TestOllamaSimpleFieldExtraction()
            {
                string json = "{\"model\": \"zephyr\", \"created_at\": \"2024-10-29T19:43:45.7743388Z\", \"response\": \"The grass appears green...\", \"done\": true, \"done_reason\": \"stop\", \"context\": [28705, 13, 28789, 28766, 1838, 28766, 28767, 13, 7638, 349, 272, 10109, 5344, 28804, 26307, 1215, 15643, 28723, 13, 2, 28705, 13, 28789, 28766, 489, 11143, 28766, 28767, 13, 1014, 10109, 8045, 5344, 1096, 378, 24345, 680, 302, 272, 5344, 275, 26795, 28713, 302, 2061, 325, 28781, 28782, 28734, 28733, 28782, 28787, 28734, 23693, 300, 2612, 28731, 477, 272, 4376, 821, 378, 10612, 1816, 28723, 851, 20757, 349, 2651, 390, 484, 5638, 3126, 19530, 28725, 264, 18958, 466, 1419, 297, 9923, 369, 18156, 706, 298, 6603, 22950, 778, 3408, 1059, 8886, 28724, 448, 21537, 28723], \"total_duration\": 21070342000, \"load_duration\": 7924431300, \"prompt_eval_count\": 30, \"prompt_eval_duration\": 2666814000, \"eval_count\": 67, \"eval_duration\": 10474129000}";
                var data = JsonHandler.Deserialize(json);

                // Extract specific fields
                var model = JsonHandler.GetField(data, "model") as string;
                var createdAt = JsonHandler.GetField(data, "created_at") as DateTime?;
                var response = JsonHandler.GetField(data, "response") as string;
                var done = JsonHandler.GetField(data, "done") as bool?;

                // Extract the context field as a List<object> and convert to List<int>
                var contextList = JsonHandler.GetField(data, "context") as List<object>;
                List<int> context = contextList?.ConvertAll(item => Convert.ToInt32(item));

                // Validate extracted fields (optional for the test)
                if (model != "zephyr" || createdAt == null || response == null || done == null || context == null)
                {
                    throw new Exception("Test of Ollama Simple Field Extraction failed.");
                }
            }

            private static void TestOllamaNestedFieldExtraction()
            {
                string json = "{\"models\":[{\"name\":\"zephyr:latest\",\"model\":\"zephyr:latest\",\"modified_at\":\"2024-10-27T11:51:03.5321962-04:00\",\"size\":4109854934,\"digest\":\"bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"}},{\"name\":\"llama3.2:latest\",\"model\":\"llama3.2:latest\",\"modified_at\":\"2024-09-30T10:37:15.6276545-04:00\",\"size\":2019393189,\"digest\":\"a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"3.2B\",\"quantization_level\":\"Q4_K_M\"}}]}";
                var data = JsonHandler.Deserialize(json);

                // Extract the 'models' list
                var models = JsonHandler.GetField(data, "models") as List<object>;
                if (models == null)
                {
                    throw new Exception("Failed to extract models list.");
                }

                foreach (var model in models)
                {
                    if (model is List<KeyValuePair<string, object>> modelData)
                    {
                        // Extract fields using GetField
                        var name = JsonHandler.GetField(modelData, "name") as string;
                        var modifiedAt = JsonHandler.GetField(modelData, "modified_at") as DateTime?;
                        var family = JsonHandler.GetField(JsonHandler.GetField(modelData, "details") as List<KeyValuePair<string, object>>, "family") as string;
                        var quantizationLevel = JsonHandler.GetField(JsonHandler.GetField(modelData, "details") as List<KeyValuePair<string, object>>, "quantization_level") as string;
                        var parameterSize = JsonHandler.GetField(JsonHandler.GetField(modelData, "details") as List<KeyValuePair<string, object>>, "parameter_size") as string;

                        // Validation for testing (checking against expected values)
                        if (name == null || modifiedAt == null || family == null || quantizationLevel == null || parameterSize == null)
                        {
                            throw new Exception($"Test failed for model '{name ?? "unknown"}': missing required fields.");
                        }

                        // Output for validation (or assertions in real test)
                        Console.WriteLine($"Model: {name}");
                        Console.WriteLine($"Modified At: {modifiedAt}");
                        Console.WriteLine($"Family: {family}");
                        Console.WriteLine($"Quantization Level: {quantizationLevel}");
                        Console.WriteLine($"Parameter Size: {parameterSize}");
                        Console.WriteLine();
                    }
                    else
                    {
                        throw new Exception("Model data format is invalid.");
                    }
                }
            }

            private static void TestOllamaModelInformationExtraction()
            {
                string json = "{\"models\":[{\"name\":\"zephyr:latest\",\"model\":\"zephyr:latest\",\"modified_at\":\"2024-10-27T11:51:03.5321962-04:00\",\"size\":4109854934,\"digest\":\"bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"}},{\"name\":\"llama3.2:latest\",\"model\":\"llama3.2:latest\",\"modified_at\":\"2024-09-30T10:37:15.6276545-04:00\",\"size\":2019393189,\"digest\":\"a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"3.2B\",\"quantization_level\":\"Q4_K_M\"}}]}";
                var data = JsonHandler.Deserialize(json);

                int modelCount = (int)JsonHandler.GetFieldByPath(data, "models.length");
                Console.WriteLine($"Total Models: {modelCount}");
                Console.WriteLine();

                for (var i = 0; i < modelCount; i++)
                {
                    var name = JsonHandler.GetFieldByPath(data, $"models[{i}].name") as string;
                    var modifiedAt = JsonHandler.GetFieldByPath(data, $"models[{i}].modified_at") as DateTime?;
                    var family = JsonHandler.GetFieldByPath(data, $"models[{i}].details.family") as string;
                    var quantizationLevel = JsonHandler.GetFieldByPath(data, $"models[{i}].details.quantization_level") as string;
                    var parameterSize = JsonHandler.GetFieldByPath(data, $"models[{i}].details.parameter_size") as string;

                    // Retrieve "families" as a List<string>
                    var families = JsonHandler.GetFieldByPath(data, $"models[{i}].details.families") as List<object>;
                    var familyList = families?.ConvertAll(f => f as string); // Convert List<object> to List<string>

                    // Validation for testing (checking against expected values)
                    if (name == null || modifiedAt == null || family == null || quantizationLevel == null || parameterSize == null)
                    {
                        throw new Exception($"Test failed for model '{name ?? "unknown"}': missing required fields.");
                    }

                    // Output for validation (or assertions in real test)
                    Console.WriteLine($"Model: {name}");
                    Console.WriteLine($"Modified At: {modifiedAt}");
                    Console.WriteLine($"Family: {family}");
                    // Output the "families" list
                    if (familyList != null)
                    {
                        Console.WriteLine("Families: " + string.Join(", ", familyList));
                    }
                    Console.WriteLine($"Quantization Level: {quantizationLevel}");
                    Console.WriteLine($"Parameter Size: {parameterSize}");
                    Console.WriteLine();
                }
            }

            private static void TestDeepCompare()
            {
                string json = "{\"models\":[{\"name\":\"zephyr:latest\",\"model\":\"zephyr:latest\",\"modified_at\":\"2024-10-27T11:51:03.5321962-04:00\",\"size\":4109854934,\"digest\":\"bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"}},{\"name\":\"llama3.2:latest\",\"model\":\"llama3.2:latest\",\"modified_at\":\"2024-09-30T10:37:15.6276545-04:00\",\"size\":2019393189,\"digest\":\"a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72\",\"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",\"families\":[\"llama\"],\"parameter_size\":\"3.2B\",\"quantization_level\":\"Q4_K_M\"}}]}";
                var data = JsonHandler.Deserialize(json);

                var shouldBeData = JsonBuilder.CreateAnonymousObject(
                    JsonBuilder.CreateField("models", JsonBuilder.CreateArray(
                        // First model object
                        JsonBuilder.CreateAnonymousObject(
                            JsonBuilder.CreateField("name", "zephyr:latest"),
                            JsonBuilder.CreateField("model", "zephyr:latest"),
                            JsonBuilder.CreateField("modified_at", DateTime.Parse("2024-10-27T11:51:03.5321962-04:00")),
                            JsonBuilder.CreateNumeric("size", 4109854934L),
                            JsonBuilder.CreateField("digest", "bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354"),
                            JsonBuilder.CreateObject("details",
                                JsonBuilder.CreateField("parent_model", ""),
                                JsonBuilder.CreateField("format", "gguf"),
                                JsonBuilder.CreateField("family", "llama"),
                                JsonBuilder.CreateArray("families", "llama"),
                                JsonBuilder.CreateField("parameter_size", "7B"),
                                JsonBuilder.CreateField("quantization_level", "Q4_0")
                            )
                        ),
                        // Second model object
                        JsonBuilder.CreateAnonymousObject(
                            JsonBuilder.CreateField("name", "llama3.2:latest"),
                            JsonBuilder.CreateField("model", "llama3.2:latest"),
                            JsonBuilder.CreateField("modified_at", DateTime.Parse("2024-09-30T10:37:15.6276545-04:00")),
                            JsonBuilder.CreateNumeric("size", 2019393189L),
                            JsonBuilder.CreateField("digest", "a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72"),
                            JsonBuilder.CreateObject("details",
                                JsonBuilder.CreateField("parent_model", ""),
                                JsonBuilder.CreateField("format", "gguf"),
                                JsonBuilder.CreateField("family", "llama"),
                                JsonBuilder.CreateArray("families", "llama"),
                                JsonBuilder.CreateField("parameter_size", "3.2B"),
                                JsonBuilder.CreateField("quantization_level", "Q4_K_M")
                            )
                        )
                    ))
                );

                var shouldBeJson = JsonHandler.Serialize(shouldBeData);

                if (!JsonTestHelpers.DeepCompare(data, shouldBeData, out string difference))
                {
                    throw new DeserializationMismatchException($"Test Ollama Tag Deserialization deep compare failed. Difference: {difference}");
                }

                if (json != shouldBeJson)
                {
                    Debug.WriteLine($"Original JSON string: \n{json}");
                    Debug.WriteLine($"Re-serialized shouldBeData: \n{shouldBeJson}");

                    throw new DeserializationMismatchException($"Test Ollama Tag Deserialization re-serialization comparison failed!");
                }
            }

        }
    }
}
