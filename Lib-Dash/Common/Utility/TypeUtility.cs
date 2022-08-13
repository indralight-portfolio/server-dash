using System;
namespace Common.Utility
{
    public static class TypeUtility
    {
        public static string MakeGenericName(this Type type)
        {
            if (type.FullName == null)
            {
                throw new Exception($"Type {type} FullName is null!");
            }

            string friendlyName = type.FullName;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = MakeGenericName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName.Replace('+', '.');
        }

        public static string PrettyName(this Type type)
        {
            if (type.IsGenericType == true)
            {
                return MakeGenericName(type);
            }

            if (type.FullName == null)
            {
                throw new Exception($"Type {type} FullName is null!");
            }

            return type.FullName.Replace('+', '.');
        }

        public static T Convert<T>(object value)
        {
            try
            {
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static Type GetUnderlyingType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType ?? type;
        }

        public static Type GetGenericBaseTypeRecursive(Type source, Type parent)
        {
            if (source.BaseType == null)
            {
                return null;
            }

            if (source.IsGenericType && source.GetGenericTypeDefinition() == parent)
                return source;

            return GetGenericBaseTypeRecursive(source.BaseType, parent);
        }
    }
}