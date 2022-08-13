using MessagePack;
#if Common_Unity
using Common.Unity.Asset;
using UnityEngine;
#endif

namespace Common.Locale
{
    [MessagePackObject()]
    public class Locale
    {
        [Key(0)]
        public string Key { get; set; }

        public Locale()
        {
            Key = string.Empty;
        }

        public Locale(string key)
        {
            Key = key;
        }

        public static implicit operator Locale(string s)
        {
            return new Locale(s);
        }

        public virtual string GetValue()
        {
            return LocaleManager.Instance.Get(this);
        }

        public virtual string GetValue(bool logging)
        {
            return LocaleManager.Instance.Get(this, logging);
        }

        public virtual bool TryGet(out string value)
        {
            return LocaleManager.Instance.TryGet(this, out value);
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(Key);
        }
    }

    [MessagePackObject()]
    public class LocaleWithArgs : Locale
    {
        [Key(1)]
        public object[] Args
        {
            get { return _args; }
            set { _args = value; }
        }
        private object[] _args;

        public LocaleWithArgs() : base() { }
        public LocaleWithArgs(string key, params object[] args) : base(key)
        {
            _args = args;
        }
        public LocaleWithArgs(Locale locale, params object[] args) : this(locale.Key, args) { }

        public void SetArgs(params object[] args)
        {
            _args = args;
        }

        public static implicit operator LocaleWithArgs(string s)
        {
            return new LocaleWithArgs(s);
        }

        public override string GetValue()
        {
            return LocaleManager.Instance.Get(this, false, _args);
        }

        public override string GetValue(bool logging)
        {
            return LocaleManager.Instance.Get(this, logging, _args);
        }

        public override bool TryGet(out string value)
        {
            return LocaleManager.Instance.TryGet(this, out value, _args);
        }
    }

    public class TextAssetLocale
    {
        public string Key { get; set; }

        public TextAssetLocale()
        {
            Key = string.Empty;
        }

        public TextAssetLocale(string key)
        {
            Key = key;
        }

        public static implicit operator TextAssetLocale(string s)
        {
            return new TextAssetLocale(s);
        }

        public string GetValue()
        {
            TryGet(out string value);
            return value;
        }

        public bool TryGet(out string value)
        {
#if Common_Unity
            var language = LocaleManager.Instance.CurrentLanguage;
            var path = $"data-dash/text/{language.ToString().ToLower()}:{Key}.txt";
            var textAsset = AssetManager.Instance.Load<TextAsset>(path);
            value = textAsset?.text;

            return textAsset == null;
#else
            value = null;
            return false;
#endif
        }

        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(Key);
        }
    }
}