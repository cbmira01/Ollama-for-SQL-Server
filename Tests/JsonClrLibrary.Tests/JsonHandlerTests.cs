using System;
using System.Collections.Generic;
using Xunit;
using JsonClrLibrary;

namespace JsonClrLibrary.Tests
{
    public class JsonHandlerTests
    {
        [Fact]
        public void Serialize_ReturnsExpectedJsonString()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", "Alice"),
                new KeyValuePair<string, object>("age", 30)
            };

            string result = JsonHandler.Serialize(data);

            Assert.Equal("{\"name\":\"Alice\",\"age\":30}", result);
        }

        [Fact]
        public void Deserialize_ReturnsExpectedKeyValuePairList()
        {
            string json = "{\"name\":\"Alice\",\"age\":30}";
            var result = JsonHandler.Deserialize(json);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, kvp => kvp.Key == "name" && kvp.Value.Equals("Alice"));
            Assert.Contains(result, kvp => kvp.Key == "age" && kvp.Value.Equals(30L)); // Deserialized integers may be interpreted as longs
        }

        [Fact]
        public void DumpJson_PrintsExpectedOutput()
        {
            // Arrange
            string json = "{\"name\":\"Alice\",\"age\":30}";
            // Redirect Console Output (in a real test, check the output manually or via a console output interceptor)
            using (var consoleOutput = new System.IO.StringWriter())
            {
                Console.SetOut(consoleOutput);

                // Act
                JsonHandler.DumpJson(json);

                // Assert: Checking the formatted JSON is outputted; here we check for key phrases
                string output = consoleOutput.ToString();
                Assert.Contains("\"name\": \"Alice\"", output);
                Assert.Contains("\"age\": 30", output);
            }
        }

        [Fact]
        public void GetField_ReturnsExpectedValue()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", "Alice"),
                new KeyValuePair<string, object>("age", 30),
                new KeyValuePair<string, object>("birthdate", "2000-01-01")
            };

            Assert.Equal("Alice", JsonHandler.GetField(data, "name"));
            Assert.Equal(30, JsonHandler.GetField(data, "age"));
            Assert.Equal(new DateTime(2000, 1, 1), JsonHandler.GetField(data, "birthdate"));
        }

        [Fact]
        public void GetIntegerField_ReturnsExpectedInteger()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("age", 30)
            };

            int result = JsonHandler.GetIntegerField(data, "age");

            Assert.Equal(30, result);
        }

        [Fact]
        public void GetLongField_ReturnsExpectedLong()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("population", 10000000000L)
            };

            long result = JsonHandler.GetLongField(data, "population");

            Assert.Equal(10000000000L, result);
        }

        [Fact]
        public void GetBooleanField_ReturnsExpectedBoolean()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("isAdmin", true)
            };

            bool result = JsonHandler.GetBooleanField(data, "isAdmin");

            Assert.True(result);
        }

        [Fact]
        public void GetDateField_ReturnsExpectedDate()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("dateOfBirth", "1990-01-01")
            };

            DateTime result = JsonHandler.GetDateField(data, "dateOfBirth");

            Assert.Equal(new DateTime(1990, 1, 1), result);
        }

        [Fact]
        public void GetStringField_ReturnsExpectedString()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("city", "New York")
            };

            string result = JsonHandler.GetStringField(data, "city");

            Assert.Equal("New York", result);
        }

        [Fact]
        public void GetIntegerArray_ReturnsExpectedIntegerList()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("scores", new List<object> { 1, 2, 3 })
            };

            List<int> result = JsonHandler.GetIntegerArray(data, "scores");

            Assert.Equal(new List<int> { 1, 2, 3 }, result);
        }

        [Fact]
        public void GetStringArray_ReturnsExpectedStringList()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("cities", new List<object> { "New York", "London", "Paris" })
            };

            List<string> result = JsonHandler.GetStringArray(data, "cities");

            Assert.Equal(new List<string> { "New York", "London", "Paris" }, result);
        }

        [Fact]
        public void GetFieldByPath_ReturnsExpectedValue()
        {
            var nestedData = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("nested", new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("name", "NestedName")
                })
            };

            object result = JsonHandler.GetFieldByPath(nestedData, "nested.name");

            Assert.Equal("NestedName", result);
        }

        [Fact]
        public void GetIntegerByPath_ReturnsExpectedInteger()
        {
            var nestedData = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("nested", new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("age", 25)
                })
            };

            int result = JsonHandler.GetIntegerByPath(nestedData, "nested.age");

            Assert.Equal(25, result);
        }

        [Fact]
        public void GetDateArrayByPath_ReturnsExpectedDateList()
        {
            var nestedData = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("dates", new List<object> { "2021-01-01", "2022-01-01" })
            };

            List<DateTime> result = JsonHandler.GetDateArrayByPath(nestedData, "dates");

            Assert.Equal(new List<DateTime> { new DateTime(2021, 1, 1), new DateTime(2022, 1, 1) }, result);
        }

        [Fact]
        public void GetDoubleByPath_ReturnsExpectedDouble()
        {
            var data = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("nested", new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("value", 3.14159)
                })
            };

            double result = JsonHandler.GetDoubleByPath(data, "nested.value");

            Assert.Equal(3.14159, result, 5);
        }
    }
}
