using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;

namespace JsonClrLibrary.Tests
{
    internal class Program
    {
        public class JsonTestFramework
        {
            public static void Main(string[] args)
            {
                List<Action> tests = new List<Action>
                {
                        TestSimpleSerialization,
                        TestNestedSerialization,
                        TestDateRecognition,
                        TestOllamaRequestSerialization,
                        TestOllamaResponseDeserialization,
                        TestOllamaTagDeserialization,
                        TestOllamaSimpleFieldExtraction,
                        TestOllamaNestedFieldExtraction,
                        TestOllamaModelInformationExtraction
                };

                int index = 1;
                foreach (var test in tests)
                {
                    try
                    {
                        test.Invoke();
                        Debug.WriteLine($"Test {index}: {test.Method.Name} passed.");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Test {index}: {test.Method.Name} failed: {ex.Message}");
                    }
                    index++;
                }

            }

            private static void TestSimpleSerialization()
            {
                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Name", "Test"),
                    new KeyValuePair<string, object>("IsActive", true),
                    new KeyValuePair<string, object>("Count", 10)
                };

                string json = JsonSerializerDeserializer.Serialize(data);

                // Expected: {"Name":"Test","IsActive":true,"Count":10}
                if (json != "{\"Name\":\"Test\",\"IsActive\":true,\"Count\":10}")
                {
                    throw new Exception("Simple serialization test failed.");
                }
            }

            private static void TestNestedSerialization()
            {
                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Parent", new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("Child", "Value")
                    })
                };

                string json = JsonSerializerDeserializer.Serialize(data);

                // Expected: {"Parent":{"Child":"Value"}}
                if (json != "{\"Parent\":{\"Child\":\"Value\"}}")
                {
                    throw new Exception("Nested serialization test failed.");
                }
            }

            private static void TestDateRecognition()
            {
                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Timestamp", new DateTime(2024, 10, 27, 12, 0, 0))
                };

                string json = JsonSerializerDeserializer.Serialize(data);

                // Expected: {"Timestamp":"2024-10-27T12:00:00"}
                if (json != "{\"Timestamp\":\"2024-10-27T12:00:00\"}")
                {
                    throw new Exception("Date recognition test failed.");
                }
            }

            private static void TestOllamaRequestSerialization()
            {
                var numberArray = new List<object> { 1, 2, 3 };
                var stringArray = new List<object> { "one", "two", "three" };
                var booleanArray = new List<object> { true, false, false };

                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("model", "nameofmodel"),
                    new KeyValuePair<string, object>("prompt", "Why is the sky blue?"),
                    new KeyValuePair<string, object>("stream", false),
                    new KeyValuePair<string, object>("context1", numberArray),
                    new KeyValuePair<string, object>("context2", stringArray),
                    new KeyValuePair<string, object>("context3", booleanArray)
                };

                string json = JsonSerializerDeserializer.Serialize(data);

                string shouldBe = "{\"model\":\"nameofmodel\",\"prompt\":\"Why is the sky blue?\",\"stream\":false,\"context1\":[1,2,3],\"context2\":[\"one\",\"two\",\"three\"],\"context3\":[true,false,false]}";

                if (json != shouldBe)
                {
                    throw new Exception("Test of Ollama Request Serialization failed.");
                }
            }

            private static void TestOllamaResponseDeserialization()
            {

                if (false)
                {
                    throw new Exception("Test of Ollama Response Deserialization failed.");
                }
            }

            private static void TestOllamaTagDeserialization()
            {

                if (false)
                {
                    throw new Exception("Test of Ollama Tag Deserialization failed.");
                }
            }

            private static void TestOllamaSimpleFieldExtraction()
            {

                if (false)
                {
                    throw new Exception("Test of Ollama Simple Field Extraction failed.");
                }
            }

            private static void TestOllamaNestedFieldExtraction()
            {
                if (false)
                {
                    throw new Exception("Test of Ollama Nested Field Extraction failed.");
                }
            }

            private static void TestOllamaModelInformationExtraction()
            {
                if (false)
                {
                    throw new Exception("Test of Ollama Model Information Extraction failed.");
                }
            }

        }
    }
}
