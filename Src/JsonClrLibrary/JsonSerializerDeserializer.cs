using System;
using System.Collections.Generic;
using System.Text;

namespace JsonClrLibrary
{
    public class JsonSerializerDeserializer
    {
        public static string Serialize(List<KeyValuePair<string, object>> data)
        {
            return Serialize(data, new HashSet<string>());
        }

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
                    case double doubleVal:
                        json.Append(kvp.Value.ToString());
                        break;
                    case DateTime dateTimeVal:
                        json.Append($"\"{dateTimeVal:yyyy-MM-ddTHH:mm:ss}\"");
                        break;
                    case List<KeyValuePair<string, object>> nestedObj:
                        // Recursively call Serialize for nested objects, passing currentKeys to detect duplicates
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

        public static List<KeyValuePair<string, object>> Deserialize(string json)
        {
            int index = 0;
            return ParseObject(json, ref index);
        }

        public static void DumpJson(string json)
        {
            int indentLevel = 0;
            bool inQuotes = false;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '{':
                    case '[':
                        if (!inQuotes)
                        {
                            Console.WriteLine(new string(' ', indentLevel * 2) + ch);
                            indentLevel++;
                            Console.Write(new string(' ', indentLevel * 2));
                        }
                        else
                        {
                            Console.Write(ch);
                        }
                        break;

                    case '}':
                    case ']':
                        if (!inQuotes)
                        {
                            indentLevel--;
                            Console.WriteLine();
                            Console.Write(new string(' ', indentLevel * 2) + ch);
                        }
                        else
                        {
                            Console.Write(ch);
                        }
                        break;

                    case ',':
                        if (!inQuotes)
                        {
                            Console.WriteLine(ch);
                            Console.Write(new string(' ', indentLevel * 2));
                        }
                        else
                        {
                            Console.Write(ch);
                        }
                        break;

                    case '"':
                        Console.Write(ch);
                        inQuotes = !inQuotes;
                        break;

                    case ':':
                        if (!inQuotes)
                        {
                            Console.Write(": ");
                        }
                        else
                        {
                            Console.Write(ch);
                        }
                        break;

                    default:
                        Console.Write(ch);
                        break;
                }
            }
            Console.WriteLine(); // Ensures the console output ends on a new line
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

        private static double ParseNumber(string json, ref int index)
        {
            int start = index;
            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == '-'))
                index++;
            return double.Parse(json.Substring(start, index - start));
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

    }
}
