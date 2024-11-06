using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonClrLibrary
{
    public class JsonHandler
    {
        public static string Serialize(List<KeyValuePair<string, object>> data)
        {
            return Serialize(data, new HashSet<string>());
        }

        public static List<KeyValuePair<string, object>> Deserialize(string json)
        {
            int index = 0;
            return ParseObject(json, ref index);
        }

        #region "DumpJson"

        public static void DumpJson(string json)
        {
            int index = 0;
            var deserializedData = ParseValue(json, ref index);
            DumpValue(deserializedData, 0, inline: false);
            Console.WriteLine(); // Ensures the console cursor moves to the next line
        }

        private static void DumpValue(object value, int indentLevel, bool inline)
        {
            string indent = new string(' ', indentLevel * 2);

            switch (value)
            {
                case List<KeyValuePair<string, object>> obj:
                    Console.Write("{");
                    if (!inline) Console.WriteLine();
                    int count = obj.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var kvp = obj[i];
                        if (!inline) Console.Write(new string(' ', (indentLevel + 1) * 2));
                        Console.Write("\"" + kvp.Key + "\": ");
                        DumpValue(kvp.Value, indentLevel + 1, inline: false);
                        if (i < count - 1)
                        {
                            Console.Write(",");
                        }
                        if (!inline) Console.WriteLine();
                    }
                    if (!inline) Console.Write(new string(' ', indentLevel * 2));
                    Console.Write("}");
                    break;

                case List<object> array:
                    if (IsSimpleArray(array))
                    {
                        // Print simple arrays on one line
                        Console.Write("[");
                        for (int i = 0; i < array.Count; i++)
                        {
                            DumpValue(array[i], indentLevel + 1, inline: true);
                            if (i < array.Count - 1)
                            {
                                Console.Write(", ");
                            }
                        }
                        Console.Write("]");
                    }
                    else
                    {
                        // Print complex arrays vertically
                        Console.Write("[");
                        if (!inline) Console.WriteLine();
                        int arrCount = array.Count;
                        for (int i = 0; i < arrCount; i++)
                        {
                            if (!inline) Console.Write(new string(' ', (indentLevel + 1) * 2));
                            DumpValue(array[i], indentLevel + 1, inline: false);
                            if (i < arrCount - 1)
                            {
                                Console.Write(",");
                            }
                            if (!inline) Console.WriteLine();
                        }
                        if (!inline) Console.Write(new string(' ', indentLevel * 2));
                        Console.Write("]");
                    }
                    break;

                case string str:
                    Console.Write("\"" + EscapeString(str) + "\"");
                    break;

                case bool boolVal:
                    Console.Write(boolVal ? "true" : "false");
                    break;

                case int _:
                case long _:
                case double _:
                    Console.Write(value.ToString());
                    break;

                case DateTime dateTimeVal:
                    Console.Write("\"" + dateTimeVal.ToString("o") + "\"");
                    break;

                case null:
                    Console.Write("null");
                    break;

                default:
                    Console.Write("\"" + EscapeString(value.ToString()) + "\"");
                    break;
            }
        }

        private static bool IsSimpleType(object value)
        {
            return value is string || value is int || value is long || value is double || value is bool || value is null || value is DateTime;
        }

        private static bool IsSimpleArray(List<object> array)
        {
            return array.All(item => IsSimpleType(item));
        }

        #endregion

        #region "Get fields of object or simple type"

        public static object GetField(List<KeyValuePair<string, object>> data, string key)
        {
            foreach (var kvp in data)
            {
                if (kvp.Key == key)
                {
                    // Check if the value is a string that could be a date-time
                    if (kvp.Value is string strValue && DateTime.TryParse(strValue, out DateTime dateTimeValue))
                    {
                        return dateTimeValue; // Return as DateTime if it successfully parses
                    }
                    return kvp.Value; // Return the original value if not a date-time string
                }

                // If the value is a nested object, search recursively
                if (kvp.Value is List<KeyValuePair<string, object>> nestedData)
                {
                    var result = GetField(nestedData, key);
                    if (result != null) return result;
                }
            }

            return null; // Return null if the key is not found
        }

        public static int GetIntegerField(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is int intValue)
            {
                return intValue;
            }
            throw new InvalidCastException($"The value for '{key}' is not an integer.");
        }

        public static long GetLongField(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is long longValue)
            {
                return longValue;
            }
            throw new InvalidCastException($"The value for '{key}' is not a long.");
        }

        public static bool GetBooleanField(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is bool boolValue)
            {
                return boolValue;
            }
            throw new InvalidCastException($"The value for '{key}' is not a boolean.");
        }

        public static DateTime GetDateField(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is DateTime dateValue)
            {
                return dateValue;
            }
            if (value is string strValue && DateTime.TryParse(strValue, out DateTime parsedDate))
            {
                return parsedDate;
            }
            throw new InvalidCastException($"The value for '{key}' is not a valid DateTime.");
        }

        public static string GetStringField(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is string strValue)
            {
                return strValue;
            }
            throw new InvalidCastException($"The value for '{key}' is not a string.");
        }

        #endregion

        #region "Get arrays of object or simple types"

        public static List<int> GetIntegerArray(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is List<object> list)
            {
                return list.ConvertAll(item => Convert.ToInt32(item));
            }
            throw new InvalidCastException($"The value for '{key}' is not an array of integers.");
        }

        public static List<long> GetLongArray(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is List<object> list)
            {
                return list.ConvertAll(item => Convert.ToInt64(item));
            }
            throw new InvalidCastException($"The value for '{key}' is not an array of longs.");
        }

        public static List<bool> GetBooleanArray(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is List<object> list)
            {
                return list.ConvertAll(item => Convert.ToBoolean(item));
            }
            throw new InvalidCastException($"The value for '{key}' is not an array of booleans.");
        }

        public static List<DateTime> GetDateArray(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is List<object> list)
            {
                return list.ConvertAll(item =>
                {
                    if (item is DateTime dateTime)
                    {
                        return dateTime;
                    }
                    if (item is string strValue && DateTime.TryParse(strValue, out DateTime parsedDate))
                    {
                        return parsedDate;
                    }
                    throw new InvalidCastException("Invalid DateTime value in array.");
                });
            }
            throw new InvalidCastException($"The value for '{key}' is not an array of DateTime values.");
        }

        public static List<string> GetStringArray(List<KeyValuePair<string, object>> data, string key)
        {
            var value = GetField(data, key);
            if (value is List<object> list)
            {
                return list.ConvertAll(item => item.ToString());
            }
            throw new InvalidCastException($"The value for '{key}' is not an array of strings.");
        }

        #endregion

        #region "Get fields of object or simple types by path"

        public static object GetFieldByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var parts = path.Split('.');
            object current = data;

            foreach (var part in parts)
            {
                if (current is List<KeyValuePair<string, object>> currentData)
                {
                    var match = part;
                    int? index = null;

                    // Handle array length request
                    if (match == "length" && current is List<object> list)
                    {
                        return list.Count;
                    }

                    // Handle array indexing, e.g., "models[0]"
                    if (part.Contains("["))
                    {
                        var bracketIndex = part.IndexOf('[');
                        match = part.Substring(0, bracketIndex);
                        index = int.Parse(part.Substring(bracketIndex + 1, part.Length - bracketIndex - 2));
                    }

                    // Find the matching key-value pair safely
                    var kvp = currentData.FirstOrDefault(k => k.Key == match);
                    if (kvp.Equals(default(KeyValuePair<string, object>)))
                    {
                        throw new Exception($"Key '{match}' not found.");
                    }
                    current = kvp.Value;

                    // Apply index if it's a list of objects (array)
                    if (index.HasValue && current is List<object> listData)
                    {
                        current = listData.ElementAtOrDefault(index.Value);
                    }
                }
                else if (part == "length" && current is List<object> array)
                {
                    return array.Count; // Return count of items if ".length" is found
                }
                else
                {
                    throw new Exception($"Invalid path segment '{part}': cannot access '{current?.GetType()}' with '{part}'.");
                }
            }

            // Attempt to parse the final value as a DateTime if it's a string
            if (current is string stringValue && DateTime.TryParse(stringValue, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }

            return current;
        }

        public static int GetIntegerByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is int intValue)
            {
                return intValue;
            }
            if (value is long longValue)
            {
                return Convert.ToInt32(longValue);
            }
            if (value is double doubleValue)
            {
                return Convert.ToInt32(doubleValue);
            }
            if (value is string strValue && int.TryParse(strValue, out int parsedInt))
            {
                return parsedInt;
            }
            throw new InvalidCastException($"The value at path '{path}' cannot be converted to an integer.");
        }

        public static long GetLongByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is long longValue)
            {
                return longValue;
            }
            if (value is int intValue)
            {
                return Convert.ToInt64(intValue);
            }
            if (value is double doubleValue)
            {
                return Convert.ToInt64(doubleValue);
            }
            if (value is string strValue && long.TryParse(strValue, out long parsedLong))
            {
                return parsedLong;
            }
            throw new InvalidCastException($"The value at path '{path}' cannot be converted to a long.");
        }

        public static double GetDoubleByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is double doubleValue)
            {
                return doubleValue;
            }
            if (value is int intValue)
            {
                return Convert.ToDouble(intValue);
            }
            if (value is long longValue)
            {
                return Convert.ToDouble(longValue);
            }
            if (value is string strValue && double.TryParse(strValue, out double parsedDouble))
            {
                return parsedDouble;
            }
            throw new InvalidCastException($"The value at path '{path}' cannot be converted to a double.");
        }

        public static bool GetBooleanByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is bool boolValue)
            {
                return boolValue;
            }
            if (value is string strValue && bool.TryParse(strValue, out bool parsedBool))
            {
                return parsedBool;
            }
            throw new InvalidCastException($"The value at path '{path}' cannot be converted to a boolean.");
        }

        public static DateTime GetDateByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is DateTime dateValue)
            {
                return dateValue;
            }
            if (value is string strValue && DateTime.TryParse(strValue, out DateTime parsedDate))
            {
                return parsedDate;
            }
            throw new InvalidCastException($"The value at path '{path}' cannot be converted to a DateTime.");
        }

        public static string GetStringByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value != null)
            {
                return value.ToString();
            }
            throw new InvalidCastException($"The value at path '{path}' is null and cannot be converted to a string.");
        }

        #endregion

        #region "Get arrays of object or simple types by path"

        public static List<object> GetListByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var value = GetFieldByPath(data, path);
            if (value is List<object> list)
            {
                return list;
            }
            throw new InvalidCastException($"The value at path '{path}' is not an array.");
        }

        public static List<int> GetIntegerArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item => Convert.ToInt32(item));
        }

        public static List<long> GetLongArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item => Convert.ToInt64(item));
        }

        public static List<double> GetDoubleArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item => Convert.ToDouble(item));
        }

        public static List<bool> GetBooleanArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item => Convert.ToBoolean(item));
        }

        public static List<DateTime> GetDateArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item =>
            {
                if (item is DateTime dateTime)
                {
                    return dateTime;
                }
                if (item is string strValue && DateTime.TryParse(strValue, out DateTime parsedDate))
                {
                    return parsedDate;
                }
                throw new InvalidCastException($"Item '{item}' in array at path '{path}' cannot be converted to DateTime.");
            });
        }

        public static List<string> GetStringArrayByPath(List<KeyValuePair<string, object>> data, string path)
        {
            var list = GetListByPath(data, path);
            return list.ConvertAll(item => item.ToString());
        }

        #endregion

        #region "Helper methods"

        private static string Serialize(List<KeyValuePair<string, object>> data, HashSet<string> parentKeys)
        {
            HashSet<string> currentKeys = new HashSet<string>(parentKeys); // Track keys at this level

            StringBuilder json = new StringBuilder();
            json.Append("{");

            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];

                // Check for duplicate keys within the current level
                if (!currentKeys.Add(kvp.Key))
                {
                    throw new ArgumentException($"Duplicate key detected in data: '{kvp.Key}'");
                }

                json.Append($"\"{kvp.Key}\":");

                switch (kvp.Value)
                {
                    case string strVal:
                        json.Append($"\"{EscapeString(strVal)}\"");
                        break;
                    case bool boolVal:
                        json.Append(boolVal ? "true" : "false");
                        break;
                    case int intVal:
                    case long longVal: // Added long case here
                    case double doubleVal:
                        json.Append(kvp.Value.ToString());
                        break;
                    case DateTime dateTimeVal:
                        json.Append($"\"{dateTimeVal:o}\"");
                        break;
                    case List<KeyValuePair<string, object>> nestedObj:
                        json.Append(Serialize(nestedObj, currentKeys));
                        break;
                    case List<object> array:
                        json.Append(SerializeArray(array));
                        break;
                    default:
                        json.Append("null");
                        break;
                }

                if (i < data.Count - 1) json.Append(",");
            }

            json.Append("}");
            return json.ToString();
        }

        private static string SerializeArray(List<object> array)
        {
            StringBuilder jsonArray = new StringBuilder();
            jsonArray.Append("[");

            for (int i = 0; i < array.Count; i++)
            {
                var item = array[i];
                switch (item)
                {
                    case string strVal:
                        jsonArray.Append($"\"{EscapeString(strVal)}\"");
                        break;
                    case bool boolVal:
                        jsonArray.Append(boolVal ? "true" : "false");
                        break;
                    case int intVal:
                        jsonArray.Append(intVal.ToString());
                        break;
                    case double doubleVal:
                        jsonArray.Append(doubleVal.ToString());
                        break;
                    case DateTime dateTimeVal:
                        jsonArray.Append($"\"{dateTimeVal:yyyy-MM-ddTHH:mm:ss}\"");
                        break;
                    case List<KeyValuePair<string, object>> nestedObj:
                        jsonArray.Append(Serialize(nestedObj)); // Recursive call for nested objects
                        break;
                    case List<object> nestedArray:
                        jsonArray.Append(SerializeArray(nestedArray)); // Recursive call for arrays
                        break;
                    default:
                        jsonArray.Append("null");
                        break;
                }

                if (i < array.Count - 1) jsonArray.Append(",");
            }

            jsonArray.Append("]");
            return jsonArray.ToString();
        }

        private static string EscapeString(string input)
        {
            return input.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private static List<KeyValuePair<string, object>> ParseObject(string json, ref int index)
        {
            var result = new List<KeyValuePair<string, object>>();
            index++; // Skip '{'

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);

                // Check for closing '}' indicating end of object
                if (json[index] == '}')
                {
                    index++; // Move past '}'
                    return result;
                }

                // Parse the key, expecting it to be a string
                if (json[index] != '"')
                {
                    throw new FormatException($"Expected '\"' at index {index} for key start, found '{json[index]}'.");
                }
                var key = ParseString(json, ref index);

                SkipWhitespace(json, ref index);

                // Expect a colon after the key
                if (json[index] != ':')
                {
                    throw new FormatException($"Expected ':' at index {index} after key, found '{json[index]}'.");
                }
                index++; // Move past the colon

                SkipWhitespace(json, ref index);

                // Parse the value associated with the key
                var value = ParseValue(json, ref index);
                result.Add(new KeyValuePair<string, object>(key, value));

                SkipWhitespace(json, ref index);

                // Check for a comma or the end of the object
                if (json[index] == ',')
                {
                    index++; // Move past comma
                }
                else if (json[index] == '}')
                {
                    index++; // Move past '}'
                    return result;
                }
                else
                {
                    throw new FormatException($"Expected ',' or '}}' at index {index}, found '{json[index]}'.");
                }
            }
            throw new FormatException("Unterminated object in JSON.");
        }

        private static List<object> ParseArray(string json, ref int index)
        {
            var result = new List<object>();
            index++; // Skip '['

            while (index < json.Length && json[index] != ']')
            {
                var value = ParseValue(json, ref index);
                result.Add(value);

                if (json[index] == ',') index++; // Skip ','
            }
            index++; // Skip ']'
            return result;
        }

        private static object ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            char currentChar = json[index];

            switch (currentChar)
            {
                case '"':
                    return ParseString(json, ref index);
                case '{':
                    return ParseObject(json, ref index);
                case '[':
                    return ParseArray(json, ref index);
                case var ch when char.IsDigit(ch) || ch == '-':
                    return ParseNumber(json, ref index);
                case 't':
                case 'f':
                    return ParseBoolean(json, ref index);
                case 'n':
                    return ParseNull(json, ref index);
                default:
                    // Capture 10 characters before and after the current index, handling boundaries
                    int start = Math.Max(index - 10, 0);
                    int end = Math.Min(index + 10, json.Length - 1);
                    string context = json.Substring(start, end - start + 1);

                    throw new FormatException($"Unexpected character in JSON. Context: '{context}' (index: {index})");
            }
        }

        private static string ParseString(string json, ref int index)
        {
            StringBuilder result = new StringBuilder();
            index++; // Skip initial '"'

            while (index < json.Length && json[index] != '"')
            {
                if (json[index] == '\\')
                {
                    index++; // Skip '\'
                    if (json[index] == '"') result.Append('"');
                    else if (json[index] == 'n') result.Append('\n');
                    else if (json[index] == 'r') result.Append('\r');
                }
                else
                {
                    result.Append(json[index]);
                }
                index++;
            }
            index++; // Skip closing '"'
            return result.ToString();
        }

        private static object ParseNumber(string json, ref int index)
        {
            int start = index;
            bool isInteger = true;

            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == '-'))
            {
                if (json[index] == '.') isInteger = false;
                index++;
            }

            string numberStr = json.Substring(start, index - start);

            // If the value is a whole number and fits in a long, parse as long; otherwise, parse as double
            if (isInteger && long.TryParse(numberStr, out long longVal))
            {
                return longVal;
            }
            else
            {
                return double.Parse(numberStr);
            }
        }

        private static bool ParseBoolean(string json, ref int index)
        {
            if (json[index] == 't')
            {
                index += 4; // Skip 'true'
                return true;
            }
            else
            {
                index += 5; // Skip 'false'
                return false;
            }
        }

        private static object ParseNull(string json, ref int index)
        {
            index += 4; // Skip 'null'
            return null;
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index])) index++;
        }

        #endregion
    }
}
