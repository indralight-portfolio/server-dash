#if Admin_Server
using Common.StaticInfo;
using System;
using System.Reflection;

namespace Dash.Model.Rdb
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NoUpdateAttribute : Attribute
    {
        public NoUpdateAttribute() { }
    }

    public static class AdminModel_Extensions
    {
        public static void Update<T>(this T origin, T newData) where T : Common.Model.IModel
        {
            var myType = typeof(T);

            foreach (var prop in myType.GetProperties())
            {
                if (prop.CanWrite && prop.GetSetMethod(true).IsPublic && prop.canUpdate())
                {
                    var val = prop.GetValue(newData);
                    prop.SetValue(origin, val);
                }
            }
        }

        public static bool canUpdate(this PropertyInfo prop)
        {
            var myType = prop.ReflectedType;
            var metadata = myType.GetNestedType("Metadata", BindingFlags.NonPublic);
            if (metadata == null) return true;

            var prop_ = metadata.GetProperty(prop.Name);
            if (prop_ == null) return true;
            var attr = prop_.GetCustomAttribute<NoUpdateAttribute>();

            return attr == null;
        }
    }
}
#endif