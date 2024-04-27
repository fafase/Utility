using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tools
{
    [AddComponentMenu("UI/Localized Text UI")]
    public class LocalizedTMP_UGUI : TextMeshProUGUI
    {
        [SerializeField] private Localization m_localizer;
        [SerializeField] private string m_localizationKey;
        [SerializeField] private List<LocArgument> m_arguments;

        public IReadOnlyList<LocArgument> Arguments => m_arguments;
        public string LocalizationKey => m_localizationKey;

        protected override void Start()
        {
            base.Start();
            text = m_localizer.GetLocalization(m_localizationKey, m_arguments);
        }
        public void SetLocalizedText()
        {
            text = m_localizer.GetLocalization(m_localizationKey, m_arguments);
            ForceMeshUpdate();
        }
    }
}
