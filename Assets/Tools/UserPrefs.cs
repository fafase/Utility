using System;
using System.Threading;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Tools
{
    public class UserPrefs: IUserPrefs 
    {
        private const string UserPrefsKey = "UserPrefsKey";
        private JObject m_jsonObject;
        public bool IsDirty { get; set; }
        public bool IsInit { get; private set; }
        public string Json => m_jsonObject.ToString();

        public void Init() 
        {
            string pp = PlayerPrefs.GetString(UserPrefsKey, "{}");
            m_jsonObject = JObject.Parse(pp);
            IsInit = true;
        }

        private bool GetValue<T>(string key, out JToken token) 
        {
            token = null;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            if (!IsInit)
            {
                Debug.LogError("[UserPrefs] Attempt to access UserPrefs while not initialized");
                return false;
            }
            if (m_jsonObject == null)
            {
                return false;
            }
            return m_jsonObject.TryGetValue(key, out token);
        }

        public bool TryGetInt(string key, out int value, int defaultValue = 0)
        {
            if(GetValue<int>(key, out JToken token)) 
            {
                value = token.ToObject<int>();
                return true;
            }
            value = defaultValue;
            return false;
        }
        public bool TryGetFloat(string key, out float value, float defaultValue = 0.0f) 
        {
            if (GetValue<float>(key, out JToken token))
            {
                value = token.ToObject<float>();
                return true;
            }
            value = defaultValue;
            return false;
        }
        public bool TryGetDate(string key,out DateTime value, DateTime defaultValue = default(DateTime)) 
        {
            if (GetValue<float>(key, out JToken token))
            {
                value = DateTime.Parse(token.ToString(), Thread.CurrentThread.CurrentCulture);
                return true;
            }
            value = defaultValue;
            return false;
        }
        public bool TryGetBool(string key, out bool value, bool defaultValue = false) 
        {
            if (GetValue<bool>(key, out JToken token))
            {
                value = token.ToObject<bool>();
                return true;
            }
            value = defaultValue;
            return false;
        }
        public bool TryGetObject<T>(string key, out T value, T defaultValue) where T : class
        {
            value = defaultValue;
            if (string.IsNullOrEmpty(key)) 
            {
                return false;
            }        
            if (!IsInit) 
            {
                Debug.LogError("[UserPrefs] Attempt to access UserPrefs while not initialized");
                return false;
            }
            if (m_jsonObject == null)
            {
                return false;
            }
            if (m_jsonObject.TryGetValue(key, out JToken jtoken))
            {
                value = jtoken.ToObject<T>();
                return true;
            }
            return false;
        }
        public void SetValue(string key, object value) 
        {
            if (string.IsNullOrEmpty(key)) 
            { 
                throw new System.ArgumentNullException("Provided key is null or empty while setting value");
            }
            m_jsonObject[key] = JToken.FromObject(value);
            SaveLocal();
        }

        public bool RemoveKey(string key) => m_jsonObject.Remove(key);
        
        public void ClearUserPrefs() 
        {
            if (!IsInit) 
            {
                return;
            }
            PlayerPrefs.DeleteKey(UserPrefsKey);
            m_jsonObject = JObject.Parse("{}");
            SaveLocal();
        }


        public bool SetUserPrefsFromRemote(string remoteUserPrefs)
        {
            if (!JsonUtility.IsValidJson(remoteUserPrefs)) 
            {
                Debug.LogError("[UserPrefs] Invalid Json from remote");
                return false;
            }
            
            m_jsonObject = JObject.Parse(remoteUserPrefs);

            SaveLocal();
            return true;
        }

        public bool MergeContentFromRemote(string remoteUserPrefs)
        {
            if (!JsonUtility.IsValidJson(remoteUserPrefs))
            {
                Debug.LogError("[UserPrefs] Invalid Json from remote");
                return false;
            }
            JObject remote = JObject.Parse(remoteUserPrefs);
            m_jsonObject.Merge(remote, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });
            SaveLocal();
            return true;
        }

        private void SaveLocal() 
        {
            PlayerPrefs.SetString(UserPrefsKey, m_jsonObject.ToString());
            IsDirty = true;
        }
    }

    public interface IUserPrefs 
    {
        /// <summary>
        /// Indicates if a change was made to the content
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// Initializes the UserPrefs object from local storage
        /// </summary>
        void Init();

        /// <summary>
        /// Try and get the value matching the key. If the key exists, returns true and stored value is placed in value out parameter.
        /// If the key does not exists, returns false and defaultValue is placed in value out parameter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Key to be found in the stored content</param>
        /// <param name="value">Updated value if key is found, else value is defaultValue</param>
        /// <param name="defaultValue">Default value if key is not found</param>
        /// <returns>True if the value is found, else false</returns>
        bool TryGetObject<T>(string key, out T value, T defaultValue = null) where T : class;

        bool TryGetInt(string key, out int value, int defaultValue = 0);
        bool TryGetFloat(string key, out float value, float defaultValue = 0.0f);
        bool TryGetDate(string key, out DateTime value,  DateTime defaultValue = default(DateTime));
        bool TryGetBool(string key, out bool value, bool defaultValue = false);

        /// <summary>
        /// Update the value from the key. 
        /// Key/Value added if not stored
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, object value);

        /// <summary>
        /// Removes the key from the stored content
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if key is found and removed, false if the key does not exist</returns>
        bool RemoveKey(string key);
        /// <summary>
        /// Clears the stored content and reset a default empty json object
        /// </summary>
        void ClearUserPrefs();
        /// <summary>
        /// Sets the stored content with the remote content. 
        /// </summary>
        /// <param name="remoteUserPrefs"></param>
        /// <returns>True if the remote content is valid json and local content is updated, else false</returns>
        bool SetUserPrefsFromRemote(string remoteUserPrefs);

        /// <summary>
        /// Merge remote json file with local json file. 
        /// If a key exists in both files, remote file value is stored
        /// </summary>
        /// <param name="remoteUserPrefs"></param>
        /// <returns>True if the remote content is valid json and local content is updated, else false</returns>
        bool MergeContentFromRemote(string remoteUserPrefs);

        /// <summary>
        /// Json string of the UserPrefs object
        /// </summary>
        string Json { get; }
    }
}
