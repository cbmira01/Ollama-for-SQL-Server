using System;
using System.Collections.Generic;
using System.Text;

namespace JsonClrLibrary
{
    public class JsonSerializerDeserializer
    {
        public static string Serialize(List<KeyValuePair<string, object>> data)
        {

            // TODO: Testing reveals that the serialization routine here allows
            //          multiple JSON tags under the same name. Some sort of 
            //          dictionary should be maintained to so that an error is
            //          produced.

            StringBuilder json = new StringBuilder();
            json.Append("{");

            for (int i = 0; i < data.Count; i++)
            {
                var kvp = data[i];
                json.Append($"\"{kvp.Key}\":");

                if (kvp.Value is string)
                    json.Append($"\"{EscapeString((string)kvp.Value)}\"");
                else if (kvp.Value is bool)
                    json.Append((bool)kvp.Value ? "true" : "false");
                else if (kvp.Value is int || kvp.Value is double)
                    json.Append(kvp.Value.ToString());
                else if (kvp.Value is DateTime)
                    json.Append($"\"{((DateTime)kvp.Value).ToString("yyyy-MM-ddTHH:mm:ss")}\"");
                else if (kvp.Value is List<KeyValuePair<string, object>> nestedObj)
                    json.Append(Serialize(nestedObj)); // Recursive call for nested objects
                else if (kvp.Value is List<object> array)
                    json.Append(SerializeArray(array));
                else
                    json.Append("null");

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
                if (item is string)
                    jsonArray.Append($"\"{EscapeString((string)item)}\"");
                else if (item is bool)
                    jsonArray.Append((bool)item ? "true" : "false");
                else if (item is int || item is double)
                    jsonArray.Append(item.ToString());
                else if (item is DateTime)
                    jsonArray.Append($"\"{((DateTime)item).ToString("yyyy-MM-ddTHH:mm:ss")}\"");
                else if (item is List<KeyValuePair<string, object>> nestedObj)
                    jsonArray.Append(Serialize(nestedObj)); // Recursive call for nested objects
                else if (item is List<object> nestedArray)
                    jsonArray.Append(SerializeArray(nestedArray)); // Recursive call for arrays
                else
                    jsonArray.Append("null");

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

            while (index < json.Length && json[index] != '}')
            {
                var key = ParseString(json, ref index);
                index++; // Skip ':'

                object value = ParseValue(json, ref index);
                result.Add(new KeyValuePair<string, object>(key, value));

                if (json[index] == ',') index++; // Skip ','
            }
            index++; // Skip '}'
            return result;
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

            if (currentChar == '"') return ParseString(json, ref index);
            if (currentChar == '{') return ParseObject(json, ref index);
            if (currentChar == '[') return ParseArray(json, ref index);
            if (char.IsDigit(currentChar) || currentChar == '-') return ParseNumber(json, ref index);
            if (currentChar == 't' || currentChar == 'f') return ParseBoolean(json, ref index);
            if (currentChar == 'n') return ParseNull(json, ref index);

            throw new FormatException("Unexpected character in JSON.");
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
    } // end class
} // end namespace
