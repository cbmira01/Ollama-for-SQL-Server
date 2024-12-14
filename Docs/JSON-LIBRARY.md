# Ollama Completions for SQL Server - JSON Library

This project integrates JSON functionality with SQL Server using CLR 4, enabling 
JSON-based operations and completions in SQL Server. Due to CLR 4's lack of support 
for modern JSON serializers like Newtonsoft.Json, a custom JSON library was developed, 
relying on primitive string manipulation to construct and handle JSON data.

**in progress**

## JSON Library Overview

The JSON library was designed to meet the specific requirements of this project. It 
provides tools to build and manipulate JSON data in environments constrained by CLR 4's 
limitations.

Key Features:
- Custom JSON construction with JSON Builder.
- Efficient JSON parsing and manipulation using JSON Handler.
- Serialize, deserialize and dump JSON objects.
- Query contents of JSON objects by key or path.
- Compatibility with SQL Server CLR 4 restrictions.

---

## JSON Components

### JSON Builder

The JSON Builder is a lightweight utility for constructing JSON objects and arrays 
programmatically.

Features:

- Build JSON objects dynamically with key-value pairs.
- Construct nested JSON structures.
- Ensure JSON format validity through strict syntax enforcement.

Example Usage:
```
    var json = JsonBuilder.CreateAnonymousObject(
        JsonBuilder.CreateField("name", "Example"),
        JsonBuilder.CreateNumeric("id", 123),
        JsonBuilder.CreateObject("details",
            JsonBuilder.CreateField("type", "Sample"),
            JsonBuilder.CreateArray("tags", "test", "json")
        )
    );
```

---

### JSON Handler

The JSON Handler provides functionality to parse, query, and manipulate JSON strings. 
It is optimized for environments where modern libraries are unavailable.

Features:
- Serialize JsonBuilder objects into JSON strings.
- Deserialize JSON back to object form.
- Retrieve values by key or path.

Example Usage:
```
    string json = @"{\"models\":[{\"name\":\"zephyr:latest\",\"model\":\"zephyr:latest\",
            \"modified_at\":\"2024-10-27T11:51:03.5321962-04:00\",\"size\":4109854934,
            \"digest\":\"bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354\",
            \"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",
            \"families\":[\"llama\"],\"parameter_size\":\"7B\",\"quantization_level\":\"Q4_0\"}},
            {\"name\":\"llama3.2:latest\",\"model\":\"llama3.2:latest\",
            \"modified_at\":\"2024-09-30T10:37:15.6276545-04:00\",\"size\":2019393189,
            \"digest\":\"a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72\",
            \"details\":{\"parent_model\":\"\",\"format\":\"gguf\",\"family\":\"llama\",
            \"families\":[\"llama\"],\"parameter_size\":\"3.2B\",\"quantization_level\":\"Q4_K_M\"}}]}";

    var data = JsonHandler.Deserialize(json);

    int modelCount = (int)JsonHandler.GetFieldByPath(data, "models.length");
    var name = JsonHandler.GetFieldByPath(data, $"models[{0}].name") as string;
```

---

## Testing

Comprehensive testing of the JSON Builder and JSON Handler is documented in TESTING.md. 
This includes unit and integration tests to ensure the library handles all JSON-related 
operations reliably.

---

## API Reference

### How to include JsonClrLibrary

To include JSON support into your new SQL/CLR project, make a project reference to JsonClrLibrary.
Then, in new classes that require JSON support, include this `using` stanza at the top:

```
using JsonClrLibrary;
```

JsonBuilder and JsonHandler classes and methods can then be found. An API reference follows, below.
The solution unit and integration tests also have good examples of how to use JsonClrLibrary.

---

### API Reference to JSON Builder

The JsonBuilder class provides static methods to create JSON-like structures using key-value pairs. 
It includes functionality to create objects, arrays, and fields, with support for numeric type conversion 
and overloaded methods for flexibility.

CreateAnonymousObject(params KeyValuePair<string, object>[] fields)

    Description: Creates an anonymous object from the provided fields.

    Parameters:
        fields (KeyValuePair<string, object>[]): Fields to include in the object.

    Returns: List<KeyValuePair<string, object>> - A list of fields representing the object.

CreateField(string key, object value)

    Description: Creates a key-value pair representing a field.

    Parameters:
        key (string): The name of the field.
        value (object): The value of the field.

    Returns: KeyValuePair<string, object> - A key-value pair with the specified key and value.

CreateObject(string key, params KeyValuePair<string, object>[] fields)

    Description: Creates a key-value pair representing an object, where the value is a list of fields.

    Parameters:
        key (string): The name of the object.
        fields (KeyValuePair<string, object>[]): Fields to include in the object.

    Returns: KeyValuePair<string, object> - A key-value pair where the value is a list of fields.


CreateArray(string key, params object[] items)

    Description: Creates a key-value pair representing an array, where the value is a list of items.

    Parameters:
        key (string): The name of the array.
        items (object[]): Items to include in the array.

    Returns: KeyValuePair<string, object> - A key-value pair where the value is a list of items.

CreateArray(string key, List<int> items)

    Description: Creates a key-value pair representing an array from a list of integers.

    Parameters:
        key (string): The name of the array.
        items (List<int>): A list of integers to include in the array. If items is null, an empty list is created.

    Returns: KeyValuePair<string, object> - A key-value pair where the value is a list of integers.

CreateArray(params object[] items)

    Description: Creates an anonymous array from the provided items.

    Parameters:
        items (object[]): Items to include in the array.

    Returns: List<object> - A list of items.

CreateNumeric(string key, object value)

    Description: Creates a key-value pair where the value is ensured to be a numeric type. 
    Converts string values to numeric types if possible.

    Parameters:
        key (string): The name of the field.
        value (object): The value to be converted to a numeric type. 
        Accepts integers, floats, doubles, decimals, or strings that can be parsed into these types.

    Returns: KeyValuePair<string, object> - A key-value pair with a numeric value.

    Exceptions: Throws ArgumentException if the value cannot be converted to a recognized numeric type.

---

### API Reference to JSON Handler

The JsonHandler class provides utility methods to handle JSON serialization, deserialization, 
data retrieval, and pretty-printing. It allows working with JSON-like structures based on 
KeyValuePair<string, object> and supports nested and complex data structures.

Serialize(List<KeyValuePair<string, object>> data)

    Description: Serializes a list of key-value pairs into a JSON string.

    Parameters:
        data (List<KeyValuePair<string, object>>): The list of key-value pairs representing the JSON object.

    Returns: string - A JSON-formatted string.

    Exceptions: Throws ArgumentException if duplicate keys are detected.

Deserialize(string json)

    Description: Deserializes a JSON string into a list of key-value pairs.

    Parameters:
        json (string): The JSON string to deserialize.

    Returns: List<KeyValuePair<string, object>> - A list of key-value pairs representing the JSON object.

DumpJson(string json)

    Description: Pretty-prints a JSON string to the console with proper indentation.

    Parameters:
        json (string): The JSON string to pretty-print.

    Returns: None.

GetField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves the value associated with a specified key from a JSON-like structure.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: object - The value associated with the key, or null if not found.

GetIntegerField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves an integer value associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: int - The integer value.

    Exceptions: Throws InvalidCastException if the value is not an integer.

GetLongField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves a long integer value associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: long - The long integer value.

    Exceptions: Throws InvalidCastException if the value is not a long.

GetBooleanField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves a boolean value associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: bool - The boolean value.

    Exceptions: Throws InvalidCastException if the value is not a boolean.

GetDateField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves a DateTime value associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: DateTime - The DateTime value.

    Exceptions: Throws InvalidCastException if the value is not a valid DateTime.

GetStringField(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves a string value associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: string - The string value.

    Exceptions: Throws InvalidCastException if the value is not a string.

GetFieldByPath(List<KeyValuePair<string, object>> data, string path)

    Description: Retrieves a value from a JSON-like structure using a dot-separated path.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        path (string): The dot-separated path to the value.

    Returns: object - The value found at the path.

    Exceptions: Throws Exception if the path is invalid or the key is not found.

GetIntegerArray(List<KeyValuePair<string, object>> data, string key)

    Description: Retrieves an array of integers associated with a specified key.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        key (string): The key to search for.

    Returns: List<int> - The array of integers.

    Exceptions: Throws InvalidCastException if the value is not an array of integers.

GetIntegerByPath(List<KeyValuePair<string, object>> data, string path)

    Description: Retrieves an integer value from a JSON-like structure using a dot-separated path.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        path (string): The dot-separated path to the value.

    Returns: int - The integer value.

    Exceptions: Throws InvalidCastException if the value is not an integer.

GetStringArrayByPath(List<KeyValuePair<string, object>> data, string path)

    Description: Retrieves an array of strings from a JSON-like structure using a dot-separated path.

    Parameters:
        data (List<KeyValuePair<string, object>>): The JSON-like data structure.
        path (string): The dot-separated path to the array.

    Returns: List<string> - The array of strings.

    Exceptions: Throws InvalidCastException if the value is not an array of strings.
