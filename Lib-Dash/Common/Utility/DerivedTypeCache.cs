using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Common.Utility
{
    // no interface, abstract class included.
    public static class DerivedTypeCache
    {
        public struct DeriveTypeContext
        {
            public Assembly[] Assemblies;
            public Type[] DerivedTypes;
        }

        private static object _lockObject = new object();
        private static Dictionary<(Type, Assembly[]), DeriveTypeContext> _contexts = new Dictionary<(Type, Assembly[]), DeriveTypeContext>();
        public static ReadOnlyCollection<Type> GetDerivedTypes(Type type, params Assembly[] assemblies)
        {
            lock (_lockObject)
            {
                if (_contexts.TryGetValue((type, assemblies), out DeriveTypeContext context) == true)
                {
                    return new ReadOnlyCollection<Type>(context.DerivedTypes);
                }

                InternalInit(type, assemblies);

                return new ReadOnlyCollection<Type>(_contexts[(type, assemblies)].DerivedTypes);
            }
        }

        private static void InternalInit(Type type, Assembly[] assemblies)
        {
            DeriveTypeContext context = new DeriveTypeContext();
            if (assemblies != null && assemblies.Length > 0)
            {
                List<Type> temp = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    temp.AddRange(assembly.GetTypes().Where(t =>
                        type.IsAssignableFrom(t) && t != type && t.IsInterface == false && t.IsAbstract == false)
                        .OrderBy(t => t.Name)
                    );
                }

                context.Assemblies = assemblies;
                context.DerivedTypes = temp.ToArray();
            }
            else
            {
                context.DerivedTypes = type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && t != type && t.IsInterface == false && t.IsAbstract == false)
                    .OrderBy(t => t.Name).ToArray();
            }

            _contexts.Add((type, assemblies), context);
        }
    }
}