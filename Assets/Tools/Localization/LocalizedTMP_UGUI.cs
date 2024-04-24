using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class LocalizedTMP_UGUI : TextMeshProUGUI
    {
        [SerializeField] private string m_localizationKey;
        [SerializeField] private List<LocArgument> m_arguments;

        public IReadOnlyList<LocArgument> Arguments => m_arguments;
        public string LocalizationKey => m_localizationKey;

        protected override void Start()
        {
            base.Start();
            
        }

        public void SetText() 
        {
        
        }
    }
}
