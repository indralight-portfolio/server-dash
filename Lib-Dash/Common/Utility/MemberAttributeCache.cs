using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Common.Utility
{

    public static class MemberAttributeCache
    {
        public struct FieldAttributeContext
        {
            public object[] CustomAttributes;
            public Dictionary<Type, Attribute> Attributes;
        }

        private static object _lockObject = new object();
        private static Dictionary<MemberInfo, FieldAttributeContext> _contexts = new Dictionary<MemberInfo, FieldAttributeContext>();

        public static T GetCustomAttribute<T>(MemberInfo memberInfo) where T : Attribute
        {
            if (memberInfo == null)
            {
                return null;
            }

            lock (_lockObject)
            {
                if (_contexts.ContainsKey(memberInfo) == false)
                {
                    InternalInit(memberInfo);
                }

                Type attributeType = typeof(T);
                if (_contexts[memberInfo].Attributes.ContainsKey(attributeType) == false)
                {
                    return null;
                }

                return _contexts[memberInfo].Attributes[attributeType] as T;
            }
        }

        private static void InternalInit(MemberInfo memberInfo)
        {
            FieldAttributeContext context = new FieldAttributeContext();
            context.CustomAttributes = memberInfo.GetCustomAttributes().ToArray();
            context.Attributes = new Dictionary<Type, Attribute>();

            for (int i = 0; i < context.CustomAttributes.Length; i++)
            {
                object obj = context.CustomAttributes[i];
                context.Attributes.Add(obj.GetType(), obj as Attribute);
            }

            _contexts.Add(memberInfo, context);
        }

    }
}