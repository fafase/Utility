using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(LocalizedTMP_UGUI))]
    [CanEditMultipleObjects]
    public class LocalizedTMP_UGUI_Editor : TMP_EditorPanelUI, ITMPEditor
    {
        private SerializedProperty m_localizationKey;
        private SerializedProperty m_arguments;
        private string [] m_languages; 
        private int m_languageIndex = 0;
        private ILocalization m_localizer;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_localizationKey = serializedObject.FindProperty("m_localizationKey");
            m_arguments = serializedObject.FindProperty("m_arguments");

            m_localizer = Tools.EditorUtility.LoadScriptableObject<Localization>();
            m_localizer.InitLocalizer();

            m_languages = m_localizer.Localizations;
            m_languageIndex = Array.IndexOf(m_languages, m_localizer.Locale);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            int languageIndex = EditorGUILayout.Popup(m_languageIndex, m_languages);
            if (languageIndex != m_languageIndex) 
            {
                m_localizer.SetWithLocale(m_languages[languageIndex]);
                SetText();
                m_languageIndex = languageIndex; 
            }
            EditorGUILayout.PropertyField(m_localizationKey);
            EditorGUILayout.PropertyField(m_arguments);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Select localization"))
            {
                LocalizationWindow window = EditorWindow.GetWindow<LocalizationWindow>("Localization");
                window.SetTargetScript(this);
                window.Show();
            }
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        public void SetText() 
        {
            LocalizedTMP_UGUI targetComp = (LocalizedTMP_UGUI)target;
            string value = targetComp.LocalizationKey;
            targetComp.text = m_localizer.GetLocalization(value);
        }

        public void SetTextWithKey(string value) 
        {
            m_localizationKey.stringValue = value;
            serializedObject.ApplyModifiedProperties();
            LocalizedTMP_UGUI targetComp = (LocalizedTMP_UGUI)target;
            targetComp.text = m_localizer.GetLocalization(value);
        }
    }
    
    public interface ITMPEditor 
    {
        void SetTextWithKey(string value);
    }

    class LocalizationWindow : EditorWindow
    {
        private JObject m_defaultLocalization;
        private Dictionary<string, bool> m_expand = new Dictionary<string, bool>();
        private ITMPEditor m_tmpEditor;

        void OnEnable()
        {
            ILocalization localizer = Tools.EditorUtility.LoadScriptableObject<Localization>();
            string json = localizer.DefaultJson;
            m_defaultLocalization = JObject.Parse(json);
            SetExpandList();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(10);
            foreach (var item in m_defaultLocalization)
            {
                JToken token = item.Value;
                if (token != null && token.HasValues)
                {
                    string key = item.Key;
                    if (m_expand.TryGetValue(key, out bool value))
                    {
                        EditorGUILayout.Space(10);
                        m_expand[key] = EditorGUILayout.Foldout(value, key);
                        if (m_expand[key]) 
                        { 
                            JObject children = token.ToObject<JObject>();
                            if (children != null)
                            {
                                foreach (var child in children)
                                {
                                    DrawKeyValue(child.Key, child.Value.ToString(), key);
                                }
                            }
                        }
                    }
                }
                else 
                {
                    DrawKeyValue(item.Key, item.Value.ToString()); 
                }
            }
            this.Repaint();
        }

        private void SetExpandList() 
        {
            foreach (var item in m_defaultLocalization)
            {
                JToken token = item.Value;
                if (token != null && token.HasValues)
                {
                    m_expand.Add(item.Key, false);
                }
            }
        }

        private void DrawKeyValue(string key, string value, string parent = null) 
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(key, value);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                string str = string.IsNullOrEmpty(parent) ? key : parent + "/" + key;
                m_tmpEditor.SetTextWithKey(str);
                this.Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        public void SetTargetScript(ITMPEditor tmpEditor) => m_tmpEditor = tmpEditor;
    }
}
