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
    var json = "{\"name\": \"Example\", \"id\": 123}";
    var handler = new JsonHandler(json);
    var name = handler.GetValue("name");  // Returns "Example"
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

---

### API Reference to JSON Handler

