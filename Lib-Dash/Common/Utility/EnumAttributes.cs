using System;
using System.Collections.Generic;
using Common.StaticInfo;

namespace Common.Utility
{
    public enum TestEnum
    {
        Undefined = 0,
        [Comment("SomethingDesc")] Something,
    }
    public static class EnumMemberAttributeCache<T> where T : Attribute
    {
        private static object _lock = new object();
        private static Dictionary<(Type, string), T> _attributes = new Dictionary<(Type, string), T>();
        public static T Get(Type type, string memberName)
        {
            lock (_lock)
            {
                if (_attributes.TryGetValue((type, memberName), out T attribute) == false)
                {
                    InternalInit(type, memberName);
                    return _attributes[(type, memberName)];
                }
                else
                {
                    return attribute;
                }
            }
        }

        private static void InternalInit(Type type, string memberName)
        {
            Array values = Enum.GetValues(type);
            foreach (Enum enumValue in values)
            {
                if (enumValue.ToString() == memberName)
                {
                    var attribute = GetAttributeOfType<T>(type, enumValue);
                    _attributes.Add((type, memberName), attribute);
                }
            }
        }

        public static T GetAttributeOfType<T>(Type enumType, Enum enumValue) where T:System.Attribute
        {
            var memInfo = enumType.GetMember(enumValue.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}