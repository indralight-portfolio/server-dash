using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Common.Utility
{
    public class XmlDeserializer
    {
        private Type _type;
        private XmlDeserializerCache _cache;

        public XmlDeserializer(Type type)
        {
            _type = type;
            _cache = XmlDeserializerCache.Get(type);
        }

        public object Deserialize(TextReader textReader)
        {
            var doc = new XmlDocument();
            doc.Load(textReader);

            // List<T>가 root인 경우를 제외하고는
            // 모든 list의 item이 각각 분리되어 있는 것으로 판단됨
            bool listItemSeparated = _type.Name != "List`1";

            return TransverseNode(_type, doc.DocumentElement, listItemSeparated);
        }

        public static T Deserialize<T>(string path)
        {
            var @this = new XmlDeserializer(typeof(T));
            var reader = new StreamReader(path);
            return (T)@this.Deserialize(reader);
        }

        private object TransverseNode(Type type, XmlNode node, bool listItemSeparated = true)
        {
            object obj;

            if (type.IsPrimitive)
            {
                var parseMethod = type.GetMethod("Parse", new Type[] { typeof(string) });
                obj = parseMethod.Invoke(null, new object[] { node.InnerText });
            }
            else if (type.IsEnum)
            {
                obj = Enum.Parse(type, node.InnerText, true);
            }
            else if (type.Name == "String")
            {
                return node.InnerText;
            }
//            else if (type.Name == "LocaleType")
//            {
//                return new Common.Locale.LocaleType(node.InnerText);
//            }
//            else if(type.Name == "VAHeader")
//            {
//                return new Common.StaticData.VAHeader(node.InnerText);
//            }
            else if (listItemSeparated == false && type.Name == "List`1")
            {
                obj = Activator.CreateInstance(type);

                var argType = type.GetGenericArguments()[0];
                var addMethod = type.GetMethod("Add");
                var addArg = new object[] { null };

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    var childObj = TransverseNode(argType, childNode);

                    addArg[0] = childObj;
                    addMethod.Invoke(obj, addArg);
                }
            }
            else
            {
                var typeCache = XmlDeserializerCache.Get(type);

                if (node.Attributes != null)
                {
                    var specifiedType = node.Attributes?.GetNamedItem("xsi:type") ?? null;
                    if (specifiedType != null)
                    {
                        if (typeCache.Includes.TryGetValue(specifiedType.Value, out Type tempType))
                        {
                            type = tempType;
                            typeCache = XmlDeserializerCache.Get(type);
                        }
                    }
                }

                obj = Activator.CreateInstance(type);

                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        FieldInfo fieldInfo;
                        if (!typeCache.Fields.TryGetValue(attribute.Name, out fieldInfo))
                        {
                            continue;
                        }

                        fieldInfo.SetValue(obj, TransverseNode(fieldInfo.FieldType, attribute));
                    }
                }

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    if (childNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    FieldInfo fieldInfo;
                    if (!typeCache.Fields.TryGetValue(childNode.Name, out fieldInfo))
                    {
                        continue;
                    }
                    var fieldType = fieldInfo.FieldType;

                    // 아이템을 각각 나열한 경우가 있어..
                    // 완전히 동일한 규칙의 재귀로 만들지 못함
                    if (fieldType.Name == "List`1")
                    {
                        continue;
                    }

                    fieldInfo.SetValue(obj, TransverseNode(fieldType, childNode));
                }

                if (listItemSeparated)
                {
                    foreach (var field in typeCache.Fields)
                    {
                        var fieldName = field.Key;
                        var fieldInfo = field.Value;
                        var fieldType = fieldInfo.FieldType;

                        if (fieldType.Name != "List`1")
                        {
                            continue;
                        }

                        var list = Activator.CreateInstance(fieldType);
                        fieldInfo.SetValue(obj, list);

                        var argType = fieldType.GetGenericArguments()[0];
                        var addMethod = fieldType.GetMethod("Add");
                        var addArg = new object[] { null };

                        foreach (XmlNode childNode in node.ChildNodes)
                        {
                            if (childNode.Name != fieldName)
                            {
                                continue;
                            }

                            addArg[0] = TransverseNode(argType, childNode);
                            addMethod.Invoke(list, addArg);
                        }
                    }
                }
            }

            return obj;
        }
    }

    public class XmlDeserializerCache
    {
        private static Dictionary<Type, XmlDeserializerCache> _instances = new Dictionary<Type, XmlDeserializerCache>();

        public Dictionary<string, Type> Includes { get; private set; } = new Dictionary<string, Type>();
        public Dictionary<string, FieldInfo> Fields { get; private set; } = new Dictionary<string, FieldInfo>();

        private XmlDeserializerCache(Type type)
        {
            PrepareIncludes(type);
            PrepareFields(type);
        }

        public static XmlDeserializerCache Get(Type type)
        {
            XmlDeserializerCache cache;

            if (_instances.TryGetValue(type, out cache))
            {
                return cache;
            }

            cache = new XmlDeserializerCache(type);
            _instances.Add(type, cache);
            return cache;
        }

        private void PrepareIncludes(Type type)
        {
            var xis = type.GetCustomAttributes<XmlIncludeAttribute>(true);
            foreach (var xi in xis)
            {
                var xiType = xi.Type;
                var xiName = xiType.Name;

                var xe = xiType.GetCustomAttribute<XmlElementAttribute>(true);
                if (xe != null)
                {
                    xiName = xe.ElementName;
                }

                Includes.Add(xiName, xiType);
            }
        }

        private void PrepareFields(Type type)
        {
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var fieldName = field.Name;

                var xe = field.GetCustomAttribute<XmlElementAttribute>();
                if (xe != null)
                {
                    fieldName = xe.ElementName;
                }

                Fields.Add(fieldName, field);
            }
        }
    }
}