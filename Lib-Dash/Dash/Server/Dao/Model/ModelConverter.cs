#if Common_Server
using Common.Utility;
using Dash.Model.Rdb;
using MessagePack;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dash.Server.Dao.Model
{
    public static class ModelConverter
    {
        public class Actions
        {
            public Action<object, PropertyInfo, RedisValue> FromRedisValue;
            public Action<object, PropertyInfo, object> FromObject;
            public Func<string, object, HashEntry> ToHashEntry;

            public Actions(Action<object, PropertyInfo, RedisValue> fromRedisValue, Action<object, PropertyInfo, object> fromObject, Func<string, object, HashEntry> toHashEntry)
            {
                FromRedisValue = fromRedisValue;
                FromObject = fromObject;
                ToHashEntry = toHashEntry;
            }
        }
        public static readonly IReadOnlyDictionary<Type, Actions> ModelConvertActions;

        static ModelConverter()
        {
            var modelConvertActions = new Dictionary<Type, Actions>();
            var types = new List<Type>()
            {
                typeof(Boolean),
                typeof(Byte),
                typeof(SByte),
                typeof(Int16),
                typeof(UInt16),
                typeof(Int32),
                typeof(UInt32),
                typeof(Int64),
                typeof(UInt64),
                typeof(Single),
                typeof(Double),
                typeof(Decimal),
                typeof(DateTime),
                typeof(String),
                typeof(byte[])
            };
            types.ForEach(t => modelConvertActions.Add(t, new Actions(GetActionFromRedisValue(t), GetActionFromObject(t), GetActionToHashEntry(t))));

            ModelConvertActions = modelConvertActions;
        }

        private static Action<object, PropertyInfo, RedisValue> GetActionFromRedisValue(Type type)
        {
            if (type == typeof(Boolean))
                return (i, p, v) => { p.SetValue(i, Convert.ToBoolean((bool)v)); };
            else if (type == typeof(SByte))
                return (i, p, v) => { p.SetValue(i, Convert.ToSByte((int)v)); };
            else if (type == typeof(Byte))
                return (i, p, v) => { p.SetValue(i, Convert.ToByte((int)v)); };
            else if (type == typeof(Int16))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt16((int)v)); };
            else if (type == typeof(UInt16))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt16((int)v)); };
            else if (type == typeof(Int32))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt32((int)v)); };
            else if (type == typeof(UInt32))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt32((long)v)); };
            else if (type == typeof(Int64))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt64((long)v)); };
            else if (type == typeof(UInt64))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt64((long)v)); };
            else if (type == typeof(Single))
                return (i, p, v) => { p.SetValue(i, Convert.ToSingle((double)v)); };
            else if (type == typeof(Double))
                return (i, p, v) => { p.SetValue(i, Convert.ToDouble((double)v)); };
            else if (type == typeof(Decimal))
                return (i, p, v) => { p.SetValue(i, Convert.ToDecimal((double)v)); };
            else if (type == typeof(DateTime))
                return (i, p, v) =>
                {
                    var v_ = DateTime.Parse(v.ToString(), null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                    p.SetValue(i, v_);
                };
            else if (type == typeof(String))
                return (i, p, v) => { p.SetValue(i, v.ToString()); };
            else if (type == typeof(byte[]))
                return (i, p, v) => { p.SetValue(i, (byte[])v); };
            else
                return (i, p, v) => { };
        }
        private static Action<object, PropertyInfo, object> GetActionFromObject(Type type)
        {
            if (type == typeof(Boolean))
                return (i, p, v) => { p.SetValue(i, Convert.ToBoolean(v)); };
            else if (type == typeof(SByte))
                return (i, p, v) => { p.SetValue(i, Convert.ToSByte(v)); };
            else if (type == typeof(Byte))
                return (i, p, v) => { p.SetValue(i, Convert.ToByte(v)); };
            else if (type == typeof(Int16))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt16(v)); };
            else if (type == typeof(UInt16))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt16(v)); };
            else if (type == typeof(Int32))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt32(v)); };
            else if (type == typeof(UInt32))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt32(v)); };
            else if (type == typeof(Int64))
                return (i, p, v) => { p.SetValue(i, Convert.ToInt64(v)); };
            else if (type == typeof(UInt64))
                return (i, p, v) => { p.SetValue(i, Convert.ToUInt64(v)); };
            else if (type == typeof(Single))
                return (i, p, v) => { p.SetValue(i, Convert.ToSingle(v)); };
            else if (type == typeof(Double))
                return (i, p, v) => { p.SetValue(i, Convert.ToDouble(v)); };
            else if (type == typeof(Decimal))
                return (i, p, v) => { p.SetValue(i, Convert.ToDecimal(v)); };
            else if (type == typeof(DateTime))
                return (i, p, v) =>
                {
                    var v_ = DateTime.SpecifyKind((DateTime)v, DateTimeKind.Utc);
                    p.SetValue(i, v_);
                };
            else if (type == typeof(String))
                return (i, p, v) => { p.SetValue(i, v.ToString()); };
            else if (type == typeof(byte[]))
                return (i, p, v) => { p.SetValue(i, (byte[])v); };
            else
                return (i, p, v) => { };
        }
        private static Func<string, object, HashEntry> GetActionToHashEntry(Type type)
        {
            if (type == typeof(Boolean))
                return (n, v) => { return new HashEntry(n, Convert.ToBoolean(v)); };
            else if (type == typeof(SByte))
                return (n, v) => { return new HashEntry(n, Convert.ToSByte(v)); };
            else if (type == typeof(Byte))
                return (n, v) => { return new HashEntry(n, Convert.ToInt16(v)); };
            else if (type == typeof(Int16))
                return (n, v) => { return new HashEntry(n, Convert.ToInt16(v)); };
            else if (type == typeof(UInt16))
                return (n, v) => { return new HashEntry(n, Convert.ToInt32(v)); };
            else if (type == typeof(Int32))
                return (n, v) => { return new HashEntry(n, Convert.ToInt32(v)); };
            else if (type == typeof(UInt32))
                return (n, v) => { return new HashEntry(n, Convert.ToInt64(v)); };
            else if (type == typeof(Int64))
                return (n, v) => { return new HashEntry(n, Convert.ToInt64(v)); };
            else if (type == typeof(UInt64))
                return (n, v) => { return new HashEntry(n, Convert.ToInt64(v)); };
            else if (type == typeof(Single))
                return (n, v) => { return new HashEntry(n, Convert.ToDouble(v)); };
            else if (type == typeof(Double))
                return (n, v) => { return new HashEntry(n, Convert.ToDouble(v)); };
            else if (type == typeof(Decimal))
                return (n, v) => { return new HashEntry(n, Convert.ToDouble(v)); };
            else if (type == typeof(DateTime))
                return (n, v) => { return new HashEntry(n, ((DateTime)v).ToString_ISO()); };
            else if (type == typeof(String))
                return (n, v) => { return new HashEntry(n, v.ToString()); };
            else if (type == typeof(byte[]))
                return (n, v) => { return new HashEntry(n, (byte[])v); };
            else
                return (n, v) => { return new HashEntry(n, string.Empty); };
        }
    }

    public static class ModelConverter<T> where T : class, Common.Model.IModel
    {
        public static readonly int PropertyCount = 0;
        private static readonly NLog.Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private static readonly Type _type = typeof(T);
        private static readonly PropertyInfo[] _entryProperties;
        private static readonly IReadOnlyDictionary<string, PropertyInfo> _propertyTable;
        private static readonly IReadOnlyDictionary<PropertyInfo, Type> _typeCodeTable;


        [ThreadStatic]
        private static List<HashEntry> _hashEntries;

        static ModelConverter()
        {
            _entryProperties = _type.GetProperties().Where(p => p.GetCustomAttribute<ColumnAttribute>() != null).ToArray();
            var propertyTable = new Dictionary<string, PropertyInfo>();
            var typeCodeTable = new Dictionary<PropertyInfo, Type>();
            foreach (PropertyInfo propertyInfo in _entryProperties)
            {
                propertyTable.Add(propertyInfo.Name, propertyInfo);
                var type = TypeUtility.GetUnderlyingType(propertyInfo.PropertyType);
                typeCodeTable.Add(propertyInfo, type);
            }

            _propertyTable = propertyTable;
            _typeCodeTable = typeCodeTable;
            PropertyCount = _entryProperties.Length;
        }

        public static T FromHashEntry(HashEntry[] entries)
        {
            try
            {
                if (entries.Length <= 0)
                {
                    return default;
                }

                var result = Activator.CreateInstance(_type);
                foreach (var entry in entries)
                {
                    if (_propertyTable.TryGetValue(entry.Name, out PropertyInfo propertyInfo) == false)
                    {
                        _logger.Error($"Property not found : {entry.Name}");
                        continue;
                    }
                    if (entry.Value.IsNullOrEmpty == true)
                    {
                        continue;
                    }
                    var type = _typeCodeTable.GetValueOrDefault(propertyInfo);
                    if (ModelConverter.ModelConvertActions.TryGetValue(type, out var actions))
                    {
                        actions.FromRedisValue(result, propertyInfo, entry.Value);
                    }
                }

                return (T)result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return default;
        }

        /*public static T FromSpRowData(SpRowData spRowData)
        {
            var entries = spRowData.GetRowData;
            try
            {
                if (entries.Count <= 0)
                {
                    return default(T);
                }

                var result = Activator.CreateInstance(_type);
                foreach (var entry in entries)
                {
                    if (_propertyTable.TryGetValue(entry.Key, out PropertyInfo propertyInfo) == false)
                    {
                        throw new Exception($"Property not found : {entry.Key}");
                    }
                    if (entry.Value == null || entry.Value is DBNull)
                    {
                        continue;
                    }
                    var type = _typeCodeTable.GetValueOrDefault(propertyInfo);
                    if (ModelConverter.ModelConvertActions.TryGetValue(type, out var actions) == false)
                    {
                        throw new Exception($"Convertor not found : {propertyInfo.PropertyType} {entry.Key}");
                    }

                    actions.FromObject(result, propertyInfo, entry.Value);
                }

                return (T)result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return default(T);
        }*/

        public static HashEntry[] ToHashEntries(List<KeyValuePair<string, object>> keyValues)
        {
            if (_hashEntries == null)
            {
                _hashEntries = new List<HashEntry>(_entryProperties.Length);
            }
            _hashEntries.Clear();

            foreach (var kv in keyValues)
            {
                var name = kv.Key;
                var value = kv.Value;
                if (_propertyTable.TryGetValue(name, out PropertyInfo propertyInfo) == false)
                {
                    _logger.Error($"Property not found : {name}");
                    continue;
                }
                if (value == null)
                {
                    _hashEntries.Add(new HashEntry(name, string.Empty));
                    continue;
                }
                var type = _typeCodeTable.GetValueOrDefault(propertyInfo);
                if (ModelConverter.ModelConvertActions.TryGetValue(type, out var actions))
                {
                    var hashEntry = actions.ToHashEntry(name, value);
                    _hashEntries.Add(hashEntry);
                }
            }
            return _hashEntries.ToArray();
        }

        public static HashEntry[] ToHashEntries(T instance)
        {
            if (_hashEntries == null)
            {
                _hashEntries = new List<HashEntry>(_entryProperties.Length);
            }
            _hashEntries.Clear();

            foreach (var propertyInfo in _entryProperties)
            {
                var name = propertyInfo.Name;
                var value = propertyInfo.GetValue(instance);
                if (value == null)
                {
                    _hashEntries.Add(new HashEntry(name, string.Empty));
                    continue;
                }
                var type = _typeCodeTable.GetValueOrDefault(propertyInfo);
                if (ModelConverter.ModelConvertActions.TryGetValue(type, out var actions))
                {
                    var hashEntry = actions.ToHashEntry(name, value);
                    _hashEntries.Add(hashEntry);
                }
            }
            return _hashEntries.ToArray();
        }

        public static T FromRedisValue(RedisValue value)
        {
            var serializeType = DaoDefinition.Models[typeof(T)].SerializeType;
            switch (serializeType)
            {
                case SerializeType.Json:
                    return JsonConvert.DeserializeObject<T>(value);
                case SerializeType.MessagePack:
                    return MessagePackSerializer.Deserialize<T>(value);
                default:
                    return default;
            }
        }

        public static RedisValue ToRedisValue(T value)
        {
            var serializeType = DaoDefinition.Models[typeof(T)].SerializeType;
            switch (serializeType)
            {
                case SerializeType.Json:
                    return JsonConvert.SerializeObject(value);
                case SerializeType.MessagePack:
                    return MessagePackSerializer.Serialize(value);
                default:
                    return RedisValue.Null;
            }
        }

        public static string GetHashKeyFromSubKeys(T v)
        {
            return string.Join(":", v.GetSubKeys());
        }
        public static string GetHashKeyFromKeys(T v)
        {
            var hashKey = v.GetMainKey();
            if (v.GetSubKeys().Count > 0)
                hashKey += ":" + string.Join(":", v.GetSubKeys());
            return hashKey;
        }

        // TODO: ReadOnlySpan<KeyValuePair<string, object>> 등을 return 하도록 개선?
        public static List<KeyValuePair<string, object>> GetAllParams(T model, string postfix = "")
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

            var columnProperties = SchemaInfoResolver.GetColumnProperties(_type);
            var columnNames = SchemaInfoResolver.GetColumnNames(_type);
            for (int i = 0; i < columnProperties.Count; ++i)
            {
                result.Add(new KeyValuePair<string, object>("@" + columnNames[i] + postfix, columnProperties[i].GetValue(model)));
            }

            return result;
        }

        public static KeyValuePair<string, object> GetMainKeyParam(T model)
        {
            var params_ = GetKeyParams(model);
            if (params_.Count > 0)
                return GetKeyParams(model)[0];
            else
                return new KeyValuePair<string, object>();
        }

        public static List<KeyValuePair<string, object>> GetSubKeyParams(T model, string postfix = "")
        {
            var params_ = GetKeyParams(model, postfix);
            if (params_.Count > 1)
                return params_.GetRange(1, params_.Count - 1);
            else
                return new List<KeyValuePair<string, object>>();
        }

        public static List<KeyValuePair<string, object>> GetKeyParams(T model, string postfix = "")
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

            var columnProperties = SchemaInfoResolver.GetColumnProperties(_type);
            var columnNames = SchemaInfoResolver.GetColumnNames(_type);
            for (int i = 0; i < columnProperties.Count; ++i)
            {
                var columnName = columnNames[i];
                var property = columnProperties[i];
                var attribute = (KeyColumnAttribute)property.GetCustomAttributes(typeof(KeyColumnAttribute), false).SingleOrDefault();
                if (attribute == null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, object>("@" + columnName + postfix, property.GetValue(model)));
            }
            return result;
        }

        public static List<KeyValuePair<string, object>> GetNonKeyParams(T model, string postfix = "")
        {
            List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();

            var columnProperties = SchemaInfoResolver.GetColumnProperties(_type);
            var columnNames = SchemaInfoResolver.GetColumnNames(_type);
            for (int i = 0; i < columnProperties.Count; ++i)
            {
                var columnName = columnNames[i];
                var property = columnProperties[i];
                var attribute = (KeyColumnAttribute)property.GetCustomAttributes(typeof(KeyColumnAttribute), false).SingleOrDefault();
                if (attribute != null)
                {
                    continue;
                }
                result.Add(new KeyValuePair<string, object>("@" + columnName + postfix, property.GetValue(model)));
            }
            return result;
        }
    }
}
#endif