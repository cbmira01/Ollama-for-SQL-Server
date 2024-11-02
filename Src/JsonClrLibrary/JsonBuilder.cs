using System.Collections.Generic;
using System;

namespace JsonClrLibrary
{
    public static class JsonBuilder
    {
        public static KeyValuePair<string, object> CreateField(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        public static KeyValuePair<string, object> CreateObject(string key, params KeyValuePair<string, object>[] fields)
        {
            return new KeyValuePair<string, object>(key, new List<KeyValuePair<string, object>>(fields));
        }

        public static KeyValuePair<string, object> CreateArray(string key, params object[] items)
        {
            return new KeyValuePair<string, object>(key, new List<object>(items));
        }

        // Overloaded CreateArray method to handle List<int>
        public static KeyValuePair<string, object> CreateArray(string key, List<int> items)
        {
            // If items is null, create an empty List<object>
            var objectList = items != null
                ? new List<object>(items.ConvertAll(item => (object)item))
                : new List<object>();

            return new KeyValuePair<string, object>(key, objectList);
        }

        public static List<object> CreateArray(params object[] items)
        {
            return new List<object>(items);
        }

        public static List<KeyValuePair<string, object>> CreateAnonymousObject(params KeyValuePair<string, object>[] fields)
        {
            return new List<KeyValuePair<string, object>>(fields);
        }

        public static KeyValuePair<string, object> CreateNumeric(string key, object value)
        {
            if (value is int || value is long || value is float || value is double || value is decimal)
            {
                return new KeyValuePair<string, object>(key, value);
            }

            if (value is string strValue)
            {
                if (int.TryParse(strValue, out int intValue))
                {
                    return new KeyValuePair<string, object>(key, intValue);
                }
                else if (long.TryParse(strValue, out long longValue))
                {
                    return new KeyValuePair<string, object>(key, longValue);
                }
                else if (double.TryParse(strValue, out double doubleValue))
                {
                    return new KeyValuePair<string, object>(key, doubleValue);
                }
                else if (decimal.TryParse(strValue, out decimal decimalValue))
                {
                    return new KeyValuePair<string, object>(key, decimalValue);
                }
            }

            throw new ArgumentException($"The value provided for '{key}' is not a recognized numeric type.");
        }

    } // end public class JsonSerializerDeserializer
} // end namespace JsonClrLibrary