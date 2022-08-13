using System;
using Dash.Types;

namespace Dash
{
    public class LinkInfoAttribute : Attribute
    {
        // ex : typeof(ProjectileInfo), typeof(AreaInfo), ...
        public Type InfoType { get; }
        public LinkInfoAttribute(Type type)
        {
            InfoType = type;
        }
    }

    public class LinkPrefabAttribute : Attribute
    {
        public readonly string BundleFolderPath;

        public LinkPrefabAttribute(string bundleFolderPath)
        {
            BundleFolderPath = bundleFolderPath;
        }
    }

    public class PopupEffectEditorAttribute : Attribute
    {
        // EffectInfo Id
    }

    public class DirectoryAttribute : Attribute
    {
        public DirectoryAttribute(string path)
        {
            Path = path;
        }

        public readonly string Path;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ToolLinkAttribute : Attribute
    {
        public readonly ToolType Type;
        public readonly string Path;

        public ToolLinkAttribute(ToolType type, string path)
        {
            Type = type;
            Path = path;
        }
    }

    public class LinkTextureAttribute : Attribute
    {
        public readonly string BundleFolderPath;
        public LinkTextureAttribute(string bundleFolderPath)
        {
            BundleFolderPath = bundleFolderPath;
        }
    }
    public class LinkMaterialAttribute : Attribute
    {
        public readonly string BundleFolderPath;
        public LinkMaterialAttribute(string bundleFolderPath)
        {
            BundleFolderPath = bundleFolderPath;
        }
    }
}