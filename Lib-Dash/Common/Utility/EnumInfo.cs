using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.StaticInfo;

namespace Common.Utility
{
    public static class EnumInfoNonGeneric
    {
        private static Type _enumInfoType = typeof(EnumInfo<>);
        static EnumInfoNonGeneric()
        {

        }
        public static string Get(Type type, int index)
        {
            return _enumInfoType.MakeGenericType(type).GetMethod("Get")
                .Invoke(null, new object[] { index }) as string;
        }
    }
    public static class EnumInfo<T> where T : Enum
    {
        private static ReadOnlyCollection<T> _values;
        private static ReadOnlyCollection<string> _names;
        private static ReadOnlyCollection<int> _intValues;
        private static System.Type _type;

        // thread safe.
        // https://stackoverflow.com/questions/7095/is-the-c-sharp-static-constructor-thread-safe
        static EnumInfo()
        {
            _type = typeof(T);
            System.Array rawValues = System.Enum.GetValues(_type);
            int[] intValues = new int[rawValues.Length];
            int[] indices = new int[rawValues.Length];
            for (int i = 0; i < rawValues.Length; ++i)
            {
                intValues[i] = Convert.ToInt32(rawValues.GetValue(i));
            }

            // Ascending sort by enum int value
            for (int i = 0; i < rawValues.Length; ++i)
            {
                float min = float.MaxValue;
                int minIndex = rawValues.Length;
                for (int j = 0; j < rawValues.Length; ++j)
                {
                    bool contained = false;
                    for (int k = 0; k < i; ++k)
                    {
                        if (indices[k] == j)
                        {
                            contained = true;
                            break;
                        }
                    }

                    if (contained == true)
                    {
                        continue;
                    }

                    if (min > intValues[j])
                    {
                        min = intValues[j];
                        minIndex = j;
                    }
                }

                indices[i] = minIndex;
            }

            var valueList = new List<T>(rawValues.Length);
            var namesList = new List<string>(rawValues.Length);
            var intValueList = new List<int>(rawValues.Length);

            for (int i = 0; i < intValues.Length; ++i)
            {
                valueList.Add((T)rawValues.GetValue(indices[i]));
                namesList.Add(valueList[i].ToString());
                intValueList.Add(intValues[indices[i]]);
            }

            _values = new ReadOnlyCollection<T>(valueList);
            _names = new ReadOnlyCollection<string>(namesList);
            _intValues = new ReadOnlyCollection<int>(intValueList);
        }

        public static int Count => _names.Count;
        public static string Get(int index) => _names[index];
        public static ReadOnlyCollection<string> GetNames() => _names;
        public static ReadOnlyCollection<T> GetValues() => _values;

        public static T Parse(string str)
        {
            if (TryParse(str, out T value) == true)
                return value;
            else
                throw new Exception($"[EnumInfo<{_type.Name}>] name [{str}] not found!");
        }

        public static bool TryParse(string str, out T value, StringComparison comparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < _names.Count; ++i)
            {
                if (_names[i].Equals(str, comparison))
                {
                    value = _values[i];
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public static T ConvertInt(int num)
        {
            if (TryConvertInt(num, out T value) == true)
                return value;
            else
                throw new Exception($"[EnumInfo<{nameof(T)}>] int Value {value} not found!");
        }

        public static bool TryConvertInt(int num, out T value)
        {
            for (int i = 0; i < _intValues.Count; ++i)
            {
                int current = _intValues[i];
                if (current == num)
                {
                    value = _values[i];
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public static void MakeNamesWithComment(in List<string> result)
        {
            for (int i = 0; i < _names.Count; ++i)
            {
                string name = _names[i];
                var commentAttribute = EnumMemberAttributeCache<CommentAttribute>.Get(typeof(T), name);
                if (commentAttribute != null)
                    result.Add(commentAttribute.GetString(name));
                else
                    result.Add(name);
            }
        }


        public static T ConvertByte(byte num)
        {
            return ConvertInt(num);
        }

        public static T ConvertSByte(sbyte num)
        {
            return ConvertInt(num);
        }

        public static T ConvertShort(short num)
        {
            return ConvertInt(num);
        }

        public static T ConvertUshort(ushort num)
        {
            return ConvertInt(num);
        }
        public static T Prev(T src)
        {
            T[] arr = (T[])Enum.GetValues(src.GetType());
            int index = Array.IndexOf(arr, src) - 1;
            if (index < 0)
            {
                return arr[arr.Length - 1];
            }
            if (arr.Length == index)
            {
                return arr[0];
            }
            return arr[index];
        }
        public static T Next(T src)
        {
            T[] arr = (T[])Enum.GetValues(src.GetType());
            int index = Array.IndexOf(arr, src) + 1;
            return (arr.Length == index) ? arr[0] : arr[index];
        }

        public static int IndexOf(string name)
        {
            for (int i = 0; i < _names.Count; ++i)
            {
                if (name == _names[i])
                    return i;
            }

            return -1;
        }

        public static int IndexOf(T value)
        {
            for (int i = 0; i < _values.Count; ++i)
            {
                if (value.Equals(_values[i]))
                    return i;
            }

            return -1;
        }
    }
}