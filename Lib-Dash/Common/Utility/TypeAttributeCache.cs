using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Common.Utility
{

    public static class TypeAttributeCache
    {
        public struct TypeAttributeContext
        {
            public object[] CustomAttributesInherit;
            public object[] CustomAttributesNoInherit;
            public Dictionary<Type, Attribute> Attributes;
        }

        private static object _lockObject = new object();
        private static Dictionary<Type, TypeAttributeContext> _contexts = new Dictionary<Type, TypeAttributeContext>();
        public static ReadOnlyCollection<object> GetCustomAttributesInherit(Type type)
        {
            lock (_lockObject)
            {
                if (_contexts.ContainsKey(type) == false)
                {
                    InternalInit(type);
                }

                return new ReadOnlyCollection<object>(_contexts[type].CustomAttributesInherit);
            }
        }

        public static ReadOnlyCollection<object> GetCustomAttributesNoInherit(Type type)
        {
            lock (_lockObject)
            {
                if (_contexts.ContainsKey(type) == false)
                {
                    InternalInit(type);
                }

                return new ReadOnlyCollection<object>(_contexts[type].CustomAttributesNoInherit);
            }
        }

        public static T GetCustomAttribute<T>(Type type) where T : Attribute
        {
            lock (_lockObject)
            {
                if (_contexts.ContainsKey(type) == false)
                {
                    InternalInit(type);
                }

                Type attributeType = typeof(T);
                if (_contexts[type].Attributes.ContainsKey(attributeType) == false)
                {
                    return null;
                }

                return _contexts[type].Attributes[attributeType] as T;
            }
        }

        private static void InternalInit(Type type)
        {
            TypeAttributeContext context = new TypeAttributeContext();
            context.CustomAttributesInherit = type.GetCustomAttributes(true);
            context.CustomAttributesNoInherit = type.GetCustomAttributes(false);
            context.Attributes = new Dictionary<Type, Attribute>();

            for (int i = 0; i < context.CustomAttributesInherit.Length; i++)
            {
                object obj = context.CustomAttributesInherit[i];
                context.Attributes.Add(obj.GetType(), obj as Attribute);
            }

            _contexts.Add(type, context);
        }

    }
}