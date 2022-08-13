using System;
using System.Reflection;
using Common.Utility;

namespace Common.StaticInfo
{
    public class CommentAttribute : Attribute
    {
        public string Value { get; }
        public bool ShowMemberName { get; }

        public CommentAttribute(string value, bool showMemberName = true)
        {
            Value = value;
            ShowMemberName = showMemberName;
        }

        public string GetString(MemberInfo memberInfo)
        {
            return GetString(memberInfo.Name);
        }

        public string GetString(string memberName)
        {
            if (ShowMemberName == true)
            {
                return $"{Value}[{memberName}]";
            }
            else
            {
                return Value;
            }
        }
    }

    public class HyperLinkAttribute : Attribute
    {
        public string URL { get; }

        public HyperLinkAttribute(string url)
        {
            URL = url;
        }
    }

    public class IgnoreOnEditorAttribute : Attribute
    {
    }

    // Property는 ReadOnlyOnEditorAttribute 또는 WriteOnEditorAttribute 가 있어야 Editor에서노출 된다.

    public class ReadOnlyOnEditorAttribute : Attribute
    {
    }

    public class WriteOnEditorAttribute : Attribute
    {
    }

    public class NoLocaleAttribute : Attribute
    {
    }
}