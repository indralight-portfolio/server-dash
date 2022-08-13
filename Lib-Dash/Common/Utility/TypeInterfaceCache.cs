using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Common.Utility
{

    public static class TypeInterfaceCache
    {
        public struct TypeInterfaceContext
        {
            public Type[] Interfaces;
            public Type[] GenericInterfaces;
        }

        private static object _lockObject = new object();
        private static Dictionary<Type, TypeInterfaceContext> _contexts = new Dictionary<Type, TypeInterfaceContext>();
        public static ReadOnlyCollection<Type> GetInterfaces(Type type)
        {
            lock (_lockObject)
            {
                if (_contexts.ContainsKey(type) == false)
                {
                    InternalInit(type);
                }

                return new ReadOnlyCollection<Type>(_contexts[type].Interfaces);
            }
        }

        public static ReadOnlyCollection<Type> GetGenericInterfaces(Type type)
        {
            lock (_lockObject)
            {
                if (_contexts.ContainsKey(type) == false)
                {
                    InternalInit(type);
                }

                return new ReadOnlyCollection<Type>(_contexts[type].GenericInterfaces);
            }
        }

        private static void InternalInit(Type type)
        {
            TypeInterfaceContext context = new TypeInterfaceContext();
            context.Interfaces = type.GetInterfaces();
            context.GenericInterfaces = context.Interfaces.Where(t => t.IsGenericType).ToArray();

            _contexts.Add(type, context);
        }
    }
}