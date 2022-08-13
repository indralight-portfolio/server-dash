using Common.StaticInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Common.Utility
{
    /// <summary>
    /// System.Type에 대한 각종 정보를 제공.
    /// </summary>
    public class TypeContext
    {
        public Type Type { get; private set; }
        public MemberInfo MemberInfo { get; private set; } // TODO: 이게 필요한가? 삭제 고민
        public string Name { get; private set; }
        public string Comment { get; private set; }
        public string HyperLinkURL { get; private set; }
        public ReadOnlyCollection<TypeContext> Children { get; private set; }
        public override string ToString() => $"{Type}:{Name}|Children:{Children.Count}";

        private bool _init = false;

        private static Dictionary<Type, List<TypeContext>> _childTypeContextsCache = new Dictionary<Type, List<TypeContext>>();

        public TypeContext(Type type)
        {
            Type = type;
            this.Name = type.Name;

            GetTypeInfoRecursively(type, this);
        }

        private static void GetTypeInfoRecursively(Type type, TypeContext context)
        {
            if (context._init == true)
                return;

            context._init = true;

            if (_childTypeContextsCache.TryGetValue(type, out List<TypeContext> childrenCached) == false)
            {
                List<TypeContext> list = TypeInfoHolderHelper.GetFields(type, (fieldInfo) =>
                    {
                        return fieldInfo.Attributes.HasFlag(FieldAttributes.Public) &&
                               fieldInfo.Attributes.HasFlag(FieldAttributes.Static) == false &&
                               fieldInfo.IsNotSerialized == false && fieldInfo.IsSpecialName == false;
                    })
                    .Select(fieldInfo => new TypeContext(fieldInfo.FieldType)
                    {
                        MemberInfo = fieldInfo,
                        Name = fieldInfo.Name,
                        Comment = MemberAttributeCache.GetCustomAttribute<CommentAttribute>(fieldInfo)?.GetString(fieldInfo),
                        HyperLinkURL = MemberAttributeCache.GetCustomAttribute<HyperLinkAttribute>(fieldInfo)?.URL,
                    }).ToList();

                list.AddRange(TypeInfoHolderHelper.GetProperties(type, (propertyInfo) =>
                    {
                        return propertyInfo.GetGetMethod().IsPublic == true &&
                            (propertyInfo.GetCustomAttribute<ReadOnlyOnEditorAttribute>() != null ||
                            propertyInfo.GetCustomAttribute<WriteOnEditorAttribute>() != null);
                    })
                    .Select(propertyInfo => new TypeContext(propertyInfo.PropertyType)
                    {
                        MemberInfo = propertyInfo,
                        Name = propertyInfo.Name,
                        Comment = propertyInfo.GetCustomAttribute<CommentAttribute>()?.Value,
                        HyperLinkURL = propertyInfo.GetCustomAttribute<HyperLinkAttribute>()?.URL
                    }));

                _childTypeContextsCache.Add(type, list);
                childrenCached = list;
            }

            foreach (TypeContext childCached in childrenCached)
            {
                if (childCached.MemberInfo != null)
                {
                    GetTypeInfoRecursively(childCached.Type, childCached);
                }
            }

            context.Children = new ReadOnlyCollection<TypeContext>(childrenCached);
        }
    }
}