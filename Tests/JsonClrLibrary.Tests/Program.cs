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
            public class DeserializationMismatchException : Exception
            {
                public DeserializationMismatchException(string message) : base(message) { }
            }

            public static void Main(string[] args)
            {
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
                    Debug.WriteLine("");
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

            private static void TestOneLevelDuplicateTags()
            {
                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("model", "nameofmodel"),
                    new KeyValuePair<string, object>("prompt", "Why is the sky blue?"),
                    new KeyValuePair<string, object>("stream", false),
                    new KeyValuePair<string, object>("model", "duplicateModel") // Intentional duplicate key
                };

                try
                {
                    string json = JsonSerializerDeserializer.Serialize(data);
                    throw new Exception("Test One-Level Duplicate Tags failed.");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine("Test One-Level Duplicate Tags passed: " + ex.Message);
                }
            }

            private static void TestNestedDuplicateTags()
            {
                var nestedObject = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("nestedKey1", "value1"),
                    new KeyValuePair<string, object>("nestedKey1", "duplicateValue") // Intentional duplicate key within nested object
                };

                var data = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("model", "nameofmodel"),
                    new KeyValuePair<string, object>("prompt", "Why is the sky blue?"),
                    new KeyValuePair<string, object>("nestedObject", nestedObject) // Nested object with duplicate keys
                };

                try
                {
                    string json = JsonSerializerDeserializer.Serialize(data);
                    throw new Exception("Test Nested Duplicate Tags failed.");
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine("Test Nested Duplicate Tags passed: " + ex.Message);
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
                string json = "{\"model\": \"zephyr\", \"created_at\": \"2024-10-29T19:43:45.7743388Z\", \"response\": \"The grass appears green because it reflects more of the green wavelengths of light (450-570 nanometers) from the sun than it absorbs. This phenomenon is known as chlorophyll, a pigment found in plants that enables them to convert sunlight into energy through photosynthesis.\", \"done\": true, \"done_reason\": \"stop\", \"context\": [28705, 13, 28789, 28766, 1838, 28766, 28767, 13, 7638, 349, 272, 10109, 5344, 28804, 26307, 1215, 15643, 28723, 13, 2, 28705, 13, 28789, 28766, 489, 11143, 28766, 28767, 13, 1014, 10109, 8045, 5344, 1096, 378, 24345, 680, 302, 272, 5344, 275, 26795, 28713, 302, 2061, 325, 28781, 28782, 28734, 28733, 28782, 28787, 28734, 23693, 300, 2612, 28731, 477, 272, 4376, 821, 378, 10612, 1816, 28723, 851, 20757, 349, 2651, 390, 484, 5638, 3126, 19530, 28725, 264, 18958, 466, 1419, 297, 9923, 369, 18156, 706, 298, 6603, 22950, 778, 3408, 1059, 8886, 28724, 448, 21537, 28723], \"total_duration\": 21070342000, \"load_duration\": 7924431300, \"prompt_eval_count\": 30, \"prompt_eval_duration\": 2666814000, \"eval_count\": 67, \"eval_duration\": 10474129000}";
                var data = JsonSerializerDeserializer.Deserialize(json);

                var shouldBe = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("model", "zephyr"),
                    new KeyValuePair<string, object>("created_at", DateTime.Parse("2024-10-29T19:43:45.7743388Z")),
                    new KeyValuePair<string, object>("response", "The grass appears green because it reflects more of the green wavelengths of light (450-570 nanometers) from the sun than it absorbs. This phenomenon is known as chlorophyll, a pigment found in plants that enables them to convert sunlight into energy through photosynthesis."),
                    new KeyValuePair<string, object>("done", true),
                    new KeyValuePair<string, object>("done_reason", "stop"),
                    new KeyValuePair<string, object>("context", new List<object>
                    {
                        28705, 13, 28789, 28766, 1838, 28766, 28767, 13, 7638, 349, 272, 10109, 5344, 28804, 26307, 1215,
                        15643, 28723, 13, 2, 28705, 13, 28789, 28766, 489, 11143, 28766, 28767, 13, 1014, 10109, 8045,
                        5344, 1096, 378, 24345, 680, 302, 272, 5344, 275, 26795, 28713, 302, 2061, 325, 28781, 28782,
                        28734, 28733, 28782, 28787, 28734, 23693, 300, 2612, 28731, 477, 272, 4376, 821, 378, 10612,
                        1816, 28723, 851, 20757, 349, 2651, 390, 484, 5638, 3126, 19530, 28725, 264, 18958, 466, 1419,
                        297, 9923, 369, 18156, 706, 298, 6603, 22950, 778, 3408, 1059, 8886, 28724, 448, 21537, 28723
                    }),
                    new KeyValuePair<string, object>("total_duration", 21070342000),
                    new KeyValuePair<string, object>("load_duration", 7924431300),
                    new KeyValuePair<string, object>("prompt_eval_count", 30),
                    new KeyValuePair<string, object>("prompt_eval_duration", 2666814000),
                    new KeyValuePair<string, object>("eval_count", 67),
                    new KeyValuePair<string, object>("eval_duration", 10474129000)
                };

                if (!JsonTestHelpers.DeepCompare(data, shouldBe, out string difference))
                {
                    throw new DeserializationMismatchException($"Test of Ollama Response Deserialization failed. difference: {difference}");
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
