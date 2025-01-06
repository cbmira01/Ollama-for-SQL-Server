using System;
using System.Collections.Generic;
using Xunit;
using JsonClrLibrary;

namespace JsonClrLibrary.Tests
{
    public class JsonBuilderTests
    {
        [Fact]
        public void CreateField_ReturnsKeyValuePair()
        {
            string key = "name";
            string value = "Alice";
            var result = JsonBuilder.CreateField(key, value);

            Assert.Equal(key, result.Key);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void CreateObject_ReturnsKeyValuePairWithListOfFields()
        {
            string key = "person";
            var field1 = JsonBuilder.CreateField("name", "Alice");
            var field2 = JsonBuilder.CreateField("age", 30);

            var result = JsonBuilder.CreateObject(key, field1, field2);

            Assert.Equal(key, result.Key);
            Assert.IsType<List<KeyValuePair<string, object>>>(result.Value);
            var fields = (List<KeyValuePair<string, object>>)result.Value;

            // Verify each field by checking both key and value
            Assert.True(fields.Exists(f => f.Key == field1.Key && f.Value.Equals(field1.Value)));
            Assert.True(fields.Exists(f => f.Key == field2.Key && f.Value.Equals(field2.Value)));
        }

        [Fact]
        public void CreateArray_WithParamsItems_ReturnsKeyValuePairWithList()
        {
            string key = "items";
            var item1 = "apple";
            var item2 = "banana";

            var result = JsonBuilder.CreateArray(key, item1, item2);

            Assert.Equal(key, result.Key);
            Assert.IsType<List<object>>(result.Value);
            var items = (List<object>)result.Value;
            Assert.Contains(item1, items);
            Assert.Contains(item2, items);
        }

        [Fact]
        public void CreateArray_WithListOfIntegers_ReturnsKeyValuePairWithListOfIntegers()
        {
            string key = "numbers";
            var items = new List<int> { 1, 2, 3 };

            var result = JsonBuilder.CreateArray(key, items);

            Assert.Equal(key, result.Key);
            Assert.IsType<List<object>>(result.Value);
            var objects = (List<object>)result.Value;
            Assert.Equal(items.Count, objects.Count);
            Assert.All(objects, item => Assert.IsType<int>(item));
        }

        [Fact]
        public void CreateArray_WithListOfStrings_ReturnsKeyValuePairWithListOfStrings()
        {
            string key = "strings";
            var items = new List<string> { "one", "two", "three" };

            var result = JsonBuilder.CreateArray(key, items);

            Assert.Equal(key, result.Key);
            Assert.IsType<List<object>>(result.Value);
            var objects = (List<object>)result.Value;
            Assert.Equal(items.Count, objects.Count);
            Assert.All(objects, item => Assert.IsType<string>(item));
        }

        [Fact]
        public void CreateAnonymousObject_ReturnsListOfFields()
        {
            var field1 = JsonBuilder.CreateField("name", "Alice");
            var field2 = JsonBuilder.CreateField("age", 30);

            var result = JsonBuilder.CreateAnonymousObject(field1, field2);

            Assert.IsType<List<KeyValuePair<string, object>>>(result);

            // Verify each field by checking both key and value
            Assert.True(result.Exists(f => f.Key == field1.Key && f.Value.Equals(field1.Value)));
            Assert.True(result.Exists(f => f.Key == field2.Key && f.Value.Equals(field2.Value)));
        }

        [Fact]
        public void CreateNumeric_WithValidIntValue_ReturnsKeyValuePair()
        {
            string key = "age";
            int value = 30;

            var result = JsonBuilder.CreateNumeric(key, value);

            Assert.Equal(key, result.Key);
            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void CreateNumeric_WithValidStringIntValue_ReturnsKeyValuePair()
        {
            string key = "age";
            string value = "30";

            var result = JsonBuilder.CreateNumeric(key, value);

            Assert.Equal(key, result.Key);
            Assert.Equal(30, result.Value); // Value should be parsed as int
        }

        [Fact]
        public void CreateNumeric_WithInvalidString_ThrowsArgumentException()
        {
            string key = "age";
            string invalidValue = "not a number";

            Assert.Throws<ArgumentException>(() => JsonBuilder.CreateNumeric(key, invalidValue));
        }

        [Fact]
        public void CreateNumeric_WithDecimalString_ReturnsKeyValuePair()
        {
            string key = "amount";
            string value = "123.45";

            var result = JsonBuilder.CreateNumeric(key, value);

            Assert.Equal(key, result.Key);
            Assert.Equal(123.45, result.Value); // Value should be parsed as double
        }

        [Fact]
        public void CreateNumeric_WithNonNumericType_ThrowsArgumentException()
        {
            string key = "age";
            object nonNumericValue = new object();

            Assert.Throws<ArgumentException>(() => JsonBuilder.CreateNumeric(key, nonNumericValue));
        }
    }
}
