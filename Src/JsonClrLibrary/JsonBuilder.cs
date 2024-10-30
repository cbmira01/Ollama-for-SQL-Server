using System.Collections.Generic;

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
}
