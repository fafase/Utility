using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Tools
{
    [CreateAssetMenu(fileName = "Localizer", menuName = "Tools/Localizer")]
    public class Localization : ScriptableObject, IInit, ILocalization
    {
        [SerializeField] private TextAsset m_defaultLocalization;
        [SerializeField] private List<TextAsset> m_localizations;

        public string Locale { get; private set; }

        private const string s_locale = "locale";

        private bool m_init;
        bool ILocalization.IsInit
        {
            get { return m_init; }
            set { m_init = value; }
        }
        bool IInit.IsInit => m_init;

        private JObject m_jsonLocalization;
        public string[] Localizations => m_localizations.Select(loc => loc.name).ToArray();

        public bool ShouldWaitForCompletion => false;

        public InitializationResult Init()
        {
            m_init = true;
            SetDefault();
            string locale = PlayerPrefs.GetString("PPLocale", null);
            if (string.IsNullOrEmpty(locale)) 
            {
                locale = Application.systemLanguage.ToString();
            }
            SetWithLocale(locale);
            return new InitializationResult(true, GetType().Name);
        }

        public UniTask<InitializationResult> InitAsync()
        {
            throw new System.NotImplementedException();
        }
        public void InitLocalizer() 
        {
            Init();
        }
        public void SetDefault() 
        {
            if (m_defaultLocalization == null)
            {
                throw new System.Exception("[Localization] Missing default localization");
            }
            string json = m_defaultLocalization.text;
            if (!JsonUtility.IsValidJson(json))
            {
                throw new System.Exception("[Localization] Default localization is not a valid json");
            }
            m_jsonLocalization = JObject.Parse(json);
            SetLocale();
        }

        public bool SetWithLocale(string locale) 
        {
            bool result = true;
            if (string.IsNullOrEmpty(locale)) 
            {
                Debug.LogWarning("[Localization] Missing locale, setting to default");
                SetDefault();
                result = false;
            }
            TextAsset loc = m_localizations.Find((loca) => loca.name.Equals(locale));
            if(loc == null) 
            {
                Debug.LogWarning($"[Localization] Missing localization for {locale}, setting to default");
                SetDefault();
                result = false;
            }
            else 
            {
                string json = loc.text;
                m_jsonLocalization = JObject.Parse(json);
                SetLocale();
            }
            PlayerPrefs.SetString("PPLocale", locale);
            return result;
        }

        public string GetLocalization(string key, string defaultValue = null) => GetLocalization(key, null, defaultValue);

        public string GetLocalization(string key, List<LocArgument> formats, string defaultValue = null)
        {
            if (!m_init) 
            {
                Debug.LogWarning("[Localization] Not initialized");
                return null;
            }
            if (string.IsNullOrEmpty(key)) 
            {
                return null;
            }
            string[] paths = key.Split(new char[] { '/' });
            try { 
                JToken token = m_jsonLocalization[paths[0]];
                for(int i = 1; i < paths.Length; ++i) 
                {
                    token = token[paths[i]];
                }
                string result = token.ToString();
                if (formats != null && formats.Count > 0)
                {
                    foreach (LocArgument locFormat in formats)
                    {
                        string search = "{" + locFormat.name + "}";
                        string value = string.IsNullOrEmpty(locFormat.value) ? "{ }" : locFormat.value;
                        result = result.Replace(search, value);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Localization] Could not retrieve item {key}\n{e.Message}");
            }

            return defaultValue;
        }

        public void SetLocalizationFromRemote(List<string> localizations) 
        {
            for (int i  = 0; i < localizations.Count; ++i) 
            {
                if (!JsonUtility.IsValidJson(localizations[i])) 
                {
                    Debug.LogWarning($"[Localization] Invalid json at index {i}"); 
                    continue;
                }
                JObject json = JObject.Parse(localizations[i]);
                string remoteLocale = null;
                if (json.TryGetValue(s_locale, out JToken token))
                {
                    Debug.LogWarning($"[Localization] Swapping localization content {remoteLocale}");
                    remoteLocale = token.ToObject<string>();
                    int index = m_localizations.FindIndex((loca) => loca.name == remoteLocale);
                    TextAsset ta = new TextAsset(localizations[i]);
                    ta.name = remoteLocale;
                    m_localizations[index] = ta;
                }
                else 
                {
                    m_localizations.Add(new TextAsset(localizations[i]));
                }
            }
        }

        public string DefaultJson => m_defaultLocalization?.text;

        private void SetLocale() => Locale = m_jsonLocalization["locale"].ToObject<string>();
    }
    [Serializable]
    public class LocArgument 
    {
        public string name, value;

        public LocArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }

    public interface ILocalization 
    {
        /// <summary>
        /// Is the current Localization initialized
        /// </summary>
        bool IsInit { get; set; }

        void InitLocalizer();

        /// <summary>
        /// Locale language currently used
        /// </summary>
        string Locale { get; }
        /// <summary>
        /// Set localization with provided default language (English)
        /// </summary>
        void SetDefault();

        /// <summary>
        /// Set localization with provided language, english if requested language is not found
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>True, if requested language is found, else false</returns>
        bool SetWithLocale(string locale);

        /// <summary>
        /// Get the localization string for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetLocalization(string key, string defaultValue = null);

        /// <summary>
        /// Get the localization string for the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="formats"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetLocalization(string key, List<LocArgument> formats, string defaultValue = null);
        /// <summary>
        /// Reset the localization json list from remote content
        /// </summary>
        /// <param name="localizations"></param>
        void SetLocalizationFromRemote(List<string> localizations);

        /// <summary>
        /// Get the json of the default localization
        /// </summary>
        /// <returns></returns>
        string DefaultJson { get; }

        string[] Localizations { get; }
    }
}
