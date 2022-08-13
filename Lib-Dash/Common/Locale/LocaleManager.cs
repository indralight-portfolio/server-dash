using Common.StaticInfo.Reader;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#if Common_Unity
using Common.Unity.Asset;
using UnityEngine;
#endif

namespace Common.Locale
{
    public class LocaleManager : Singleton<LocaleManager>
    {
        public enum Type
        {
            Undefined = 0,
            Resource,
            Bundle,
            Server,
        }
        public IReadOnlyList<SystemLanguage> SupportedLanguages => _supportedLanguages;
        public SystemLanguage CurrentLanguage { get; private set; }

        private List<SystemLanguage> _supportedLanguages = new List<SystemLanguage> { SystemLanguage.Korean };
        private Dictionary<string, string> _locales;

        public List<string> Keys => _locales.Keys.ToList();

        public event Action<SystemLanguage> OnLanguageChanged;

        public void Init(SystemLanguage language = SystemLanguage.Korean)
        {
            _locales = new Dictionary<string, string>();
            _supportedLanguages = new List<SystemLanguage> { language };
            CurrentLanguage = language;
        }

        public void Init(string supportedLanguagePath, SystemLanguage language)
        {
            _locales = new Dictionary<string, string>();
            InitSupportedLanguageList(supportedLanguagePath);
            CurrentLanguage = language;

            // 일반 Chinese 일 경우 ChineseSimplified 로 처리
            if (CurrentLanguage == SystemLanguage.Chinese)
                CurrentLanguage = SystemLanguage.ChineseSimplified;

            if (_supportedLanguages.Contains(CurrentLanguage) == false)
            {
                Log.Logger.Error($"Language {CurrentLanguage} not supported!");
                CurrentLanguage = SystemLanguage.English;
            }
        }

        private void InitSupportedLanguageList(string filePath)
        {
#if Common_Unity
            //filePath = Path.GetFileNameWithoutExtension(filePath);
            filePath = Path.ChangeExtension(filePath, null);
            _supportedLanguages =
                JsonReader<List<SystemLanguage>>.ReadFrom(Resources.Load<TextAsset>(filePath).text);
#elif Common_Server
            var reader = new JsonReader<List<SystemLanguage>>();
            reader.Init(filePath);
            _supportedLanguages = reader.Read(true);
#endif
        }

        public void LoadFile(string rootPath, Type type = Type.Bundle)
        {
            string data = string.Empty;
            if (type == Type.Resource)
            {
#if Common_Unity
                string filePath = CombinePath(rootPath, CurrentLanguage.ToString());
                TextAsset asset = Resources.Load<TextAsset>(filePath);
                if (asset == null)
                {
                    Log.Logger.Error($"TextAssset: {filePath} not found!");
                }
                data = asset?.text;
#endif
            }
            else if (type == Type.Bundle)
            {
#if Common_Unity
                string filePath = CombinePath(rootPath, CurrentLanguage + ".csv");
                TextAsset asset = AssetManager.Instance.Load<TextAsset>(filePath);
                if (asset == null)
                {
                    Log.Logger.Error($"TextAssset: {filePath} not found!");
                }
                data = asset?.text;
#endif                
            }
            else if (type == Type.Server)
            {
                string filePath = CombinePath(rootPath, CurrentLanguage + ".csv");
                data = File.ReadAllText(filePath);
            }
            Log.Logger.Info($"Locale[{type}] loaded: {CurrentLanguage}");
            AddLocaleData(data);
        }

        public string Get(Locale locale, bool logging = true, params object[] args)
        {
            return Get(locale.Key, logging, args);
        }

        public string Get(Locale locale, bool logging = true)
        {
            return Get(locale.Key, logging);
        }

        public string Get(string key, bool logging = true, params object[] args)
        {
            string value = Get(key, logging);
            value = FormatParams(value, args);
            return value;
        }

        public string Get(string key, bool logging = true)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return string.Empty;
            }

            if (_locales == null)
            {
                if (logging == true) Log.Logger.Error("Locale not initialized, key : " + key);
                return key;
            }

            if (_locales.ContainsKey(key) == false)
            {
#if !Common_Server
                if (logging == true) Log.Logger.Warning($"Locale not found in {CurrentLanguage}, key : {key}");
#endif
                return key;
            }

            return _locales[key];
        }

        public bool TryGet(Locale locale, out string value, params object[] args)
        {
            return TryGet(locale.Key, out value, args);
        }

        public bool TryGet(string key, out string value, params object[] args)
        {
            value = string.Empty;
            if (string.IsNullOrEmpty(key) == true)
            {
                return false;
            }

            if (_locales == null)
            {
                Log.Logger.Error("Locale not initialized, key : " + key);
                return false;
            }

            bool result = _locales.TryGetValue(key, out value);
            if (result == true)
                value = FormatParams(value, args);
            return result;
        }

        private string FormatParams(string value, params object[] args)
        {
            if (args?.Length > 0)
            {
                try
                {
                    args = args.Select(v =>
                    {
                        if (v is Locale l)
                            return Get(l, false);
                        else if (v is string s)
                            return Get(s, false);
                        else
                            return v;
                    }).ToArray();
                    return string.Format(value, args);
                }
                catch { }
            }
            return value;
        }

        private void AddLocaleData(string data)
        {
            if (string.IsNullOrEmpty(data) == true) return;
            using (StringReader reader = new StringReader(data))
            {
                string line;
                int lineNumber = 1;
                while ((line = reader.ReadLine()) != null)
                {
                    string str = line.Trim();
                    var regex = new Regex(@"^([A-Za-z0-9-_.]+),(.+)$");
                    if (regex.IsMatch(str) == true)
                    {
                        var match = regex.Match(str);
                        var k = match.Groups[1].Value;
                        var v = match.Groups[2].Value;
                        if (v.StartsWith("\"") == true)
                            v = v.Substring(1, v.Length - 1);
                        if (v.EndsWith("\"") == true)
                            v = v.Substring(0, v.Length - 1);
                        v = v.Replace("\"\"", "\""); // csv로 "를 저장하면 ""로 저장이됨.

                        if (_locales.ContainsKey(k) == true)
                        {
                            Log.Logger.Warning($"Duplicated locale key, {k}:{v}");
                            _locales[k] = v.Replace(@"\n", System.Environment.NewLine);
                        }
                        else
                        {
                            _locales.Add(k, v.Replace(@"\n", System.Environment.NewLine));
                        }
                    }
                    ++lineNumber;
                }
            }
            Log.Logger.Info($"Total locale count : {_locales.Count}");
        }

        public void AddDynamicLocale(string data)
        {
            AddLocaleData(data);
        }

        private string CombinePath(string rootpath, string subpath)
        {
            if (rootpath.EndsWith(":"))
            {
                return rootpath + subpath;
            }
            else
            {
                return Path.Combine(rootpath, subpath);
            }
        }

        public void InvokeLanguageChanged()
        {
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }
    }

    public static class LanguageExtension
    {
        public static string GetName(this SystemLanguage systemLanguage)
        {
            string name = name = systemLanguage.ToString();
            switch (systemLanguage)
            {
                case SystemLanguage.Afrikaans:
                    break;
                case SystemLanguage.Arabic:
                    name = "العربية";
                    break;
                case SystemLanguage.Basque:
                    name = "Euskara";
                    break;
                case SystemLanguage.Belarusian:
                    break;
                case SystemLanguage.Bulgarian:
                    name = "Български";
                    break;
                case SystemLanguage.Catalan:
                    name = "Català";
                    break;
                case SystemLanguage.ChineseSimplified:
                    name = "简体中文";
                    break;
                case SystemLanguage.ChineseTraditional:
                    name = "繁體中文";
                    break;
                case SystemLanguage.Czech:
                    name = "Čeština";
                    break;
                case SystemLanguage.Danish:
                    name = "Dansk";
                    break;
                case SystemLanguage.Dutch:
                    name = "Nederlands";
                    break;
                case SystemLanguage.English:
                    name = "English";
                    break;
                case SystemLanguage.Estonian:
                    name = "Eesti";
                    break;
                case SystemLanguage.Faroese:
                    break;
                case SystemLanguage.Finnish:
                    name = "Suomi";
                    break;
                case SystemLanguage.French:
                    name = "Français";
                    break;
                case SystemLanguage.German:
                    name = "Deutsche";
                    break;
                case SystemLanguage.Greek:
                    name = "Ελληνικά";
                    break;
                case SystemLanguage.Hebrew:
                    name = "עברית‏";
                    break;
                case SystemLanguage.Hungarian:
                    name = "Magyar";
                    break;
                case SystemLanguage.Icelandic:
                    name = "Íslenska";
                    break;
                case SystemLanguage.Indonesian:
                    name = "Bahasa Indonesia";
                    break;
                case SystemLanguage.Italian:
                    name = "Italiano";
                    break;
                case SystemLanguage.Japanese:
                    name = "日本語";
                    break;
                case SystemLanguage.Korean:
                    name = "한국어";
                    break;
                case SystemLanguage.Latvian:
                    name = "Latviešu";
                    break;
                case SystemLanguage.Lithuanian:
                    name = "Lietuvių";
                    break;
                case SystemLanguage.Norwegian:
                    name = "Norsk";
                    break;
                case SystemLanguage.Polish:
                    name = "Polski";
                    break;
                case SystemLanguage.Portuguese:
                    name = "Português";
                    break;
                case SystemLanguage.Romanian:
                    name = "Română";
                    break;
                case SystemLanguage.Russian:
                    name = "русский";
                    break;
                case SystemLanguage.SerboCroatian:
                    break;
                case SystemLanguage.Slovak:
                    name = "Slovenčina";
                    break;
                case SystemLanguage.Slovenian:
                    name = "Slovenski";
                    break;
                case SystemLanguage.Spanish:
                    name = "Español";
                    break;
                case SystemLanguage.Swedish:
                    name = "Svenska";
                    break;
                case SystemLanguage.Thai:
                    name = "ไทย";
                    break;
                case SystemLanguage.Turkish:
                    name = "Türkçe";
                    break;
                case SystemLanguage.Ukrainian:
                    name = "Українська";
                    break;
                case SystemLanguage.Vietnamese:
                    name = "TiếngViệt";
                    break;
            }

            return name;
        }
    }

#if Common_NetCore
    public enum SystemLanguage
    {
        //
        // 요약:
        //     Afrikaans.
        Afrikaans = 0,
        //
        // 요약:
        //     Arabic.
        Arabic = 1,
        //
        // 요약:
        //     Basque.
        Basque = 2,
        //
        // 요약:
        //     Belarusian.
        Belarusian = 3,
        //
        // 요약:
        //     Bulgarian.
        Bulgarian = 4,
        //
        // 요약:
        //     Catalan.
        Catalan = 5,
        //
        // 요약:
        //     Chinese.
        Chinese = 6,
        //
        // 요약:
        //     Czech.
        Czech = 7,
        //
        // 요약:
        //     Danish.
        Danish = 8,
        //
        // 요약:
        //     Dutch.
        Dutch = 9,
        //
        // 요약:
        //     English.
        English = 10,
        //
        // 요약:
        //     Estonian.
        Estonian = 11,
        //
        // 요약:
        //     Faroese.
        Faroese = 12,
        //
        // 요약:
        //     Finnish.
        Finnish = 13,
        //
        // 요약:
        //     French.
        French = 14,
        //
        // 요약:
        //     German.
        German = 15,
        //
        // 요약:
        //     Greek.
        Greek = 16,
        //
        // 요약:
        //     Hebrew.
        Hebrew = 17,
        Hugarian = 18,
        //
        // 요약:
        //     Hungarian.
        Hungarian = 18,
        //
        // 요약:
        //     Icelandic.
        Icelandic = 19,
        //
        // 요약:
        //     Indonesian.
        Indonesian = 20,
        //
        // 요약:
        //     Italian.
        Italian = 21,
        //
        // 요약:
        //     Japanese.
        Japanese = 22,
        //
        // 요약:
        //     Korean.
        Korean = 23,
        //
        // 요약:
        //     Latvian.
        Latvian = 24,
        //
        // 요약:
        //     Lithuanian.
        Lithuanian = 25,
        //
        // 요약:
        //     Norwegian.
        Norwegian = 26,
        //
        // 요약:
        //     Polish.
        Polish = 27,
        //
        // 요약:
        //     Portuguese.
        Portuguese = 28,
        //
        // 요약:
        //     Romanian.
        Romanian = 29,
        //
        // 요약:
        //     Russian.
        Russian = 30,
        //
        // 요약:
        //     Serbo-Croatian.
        SerboCroatian = 31,
        //
        // 요약:
        //     Slovak.
        Slovak = 32,
        //
        // 요약:
        //     Slovenian.
        Slovenian = 33,
        //
        // 요약:
        //     Spanish.
        Spanish = 34,
        //
        // 요약:
        //     Swedish.
        Swedish = 35,
        //
        // 요약:
        //     Thai.
        Thai = 36,
        //
        // 요약:
        //     Turkish.
        Turkish = 37,
        //
        // 요약:
        //     Ukrainian.
        Ukrainian = 38,
        //
        // 요약:
        //     Vietnamese.
        Vietnamese = 39,
        //
        // 요약:
        //     ChineseSimplified.
        ChineseSimplified = 40,
        //
        // 요약:
        //     ChineseTraditional.
        ChineseTraditional = 41,
        //
        // 요약:
        //     Unknown.
        Unknown = 42
    }
#endif
}