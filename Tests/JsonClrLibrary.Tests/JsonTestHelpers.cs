using System;
using System.Collections.Generic;

namespace JsonClrLibrary.Tests
{
    public static class JsonTestHelpers
    {
        public static bool DeepCompare(List<KeyValuePair<string, object>> data, List<KeyValuePair<string, object>> shouldBe, out string difference)
        {
            difference = "";

            if (data.Count != shouldBe.Count)
            {
                difference = $"Mismatch in number of elements: data has {data.Count}, shouldBe has {shouldBe.Count}.";
                return false;
            }

            for (int i = 0; i < data.Count; i++)
            {
                var dataKvp = data[i];
                var shouldBeKvp = shouldBe[i];

                if (dataKvp.Key != shouldBeKvp.Key)
                {
                    difference = $"Mismatch at key '{dataKvp.Key}': Expected key '{shouldBeKvp.Key}', found '{dataKvp.Key}'.";
                    return false;
                }

                if (!ValuesAreEqual(dataKvp.Value, shouldBeKvp.Value, out string valueDifference))
                {
                    difference = $"Mismatch at key '{dataKvp.Key}': {valueDifference}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValuesAreEqual(object value1, object value2, out string valueDifference)
        {
            valueDifference = "";

            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null)
            {
                valueDifference = $"Expected '{value2 ?? "null"}', found '{value1 ?? "null"}'.";
                return false;
            }

            // Handle DateTime and string date comparison
            if (value1 is DateTime date1 && value2 is string dateStr2 && DateTime.TryParse(dateStr2, out DateTime parsedDate2))
            {
                if (date1 != parsedDate2)
                {
                    valueDifference = $"Expected '{date1}', found '{parsedDate2}'.";
                    return false;
                }
                return true;
            }
            else if (value1 is string dateStr1 && DateTime.TryParse(dateStr1, out DateTime parsedDate1) && value2 is DateTime date2)
            {
                if (parsedDate1 != date2)
                {
                    valueDifference = $"Expected '{parsedDate1}', found '{date2}'.";
                    return false;
                }
                return true;
            }

            // Handle numeric type comparison (e.g., int vs long)
            if (IsNumericType(value1) && IsNumericType(value2))
            {
                if (Convert.ToDouble(value1) != Convert.ToDouble(value2))
                {
                    valueDifference = $"Expected '{value2}', found '{value1}'.";
                    return false;
                }
                return true;
            }

            if (value1 is List<KeyValuePair<string, object>> list1 && value2 is List<KeyValuePair<string, object>> list2)
            {
                return DeepCompare(list1, list2, out valueDifference);
            }
            else if (value1 is List<object> array1 && value2 is List<object> array2)
            {
                return ArraysAreEqual(array1, array2, out valueDifference);
            }
            else if (!value1.Equals(value2))
            {
                valueDifference = $"Expected '{value2}', found '{value1}'.";
                return false;
            }

            return true;
        }

        public static bool ArraysAreEqual(List<object> array1, List<object> array2, out string arrayDifference)
        {
            arrayDifference = "";

            if (array1.Count != array2.Count)
            {
                arrayDifference = $"Array length mismatch: Expected {array2.Count}, found {array1.Count}.";
                return false;
            }

            for (int i = 0; i < array1.Count; i++)
            {
                if (!ValuesAreEqual(array1[i], array2[i], out string elementDifference))
                {
                    arrayDifference = $"Array element mismatch at index {i}: {elementDifference}";
                    return false;
                }
            }

            return true;
        }

        public static bool IsNumericType(object obj)
        {
            return obj is sbyte || obj is byte || obj is short || obj is ushort ||
                   obj is int || obj is uint || obj is long || obj is ulong ||
                   obj is float || obj is double || obj is decimal;
        }
    }
}

