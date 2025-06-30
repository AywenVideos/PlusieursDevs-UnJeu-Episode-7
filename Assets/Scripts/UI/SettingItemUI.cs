using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Slider))]
    public class SettingItemUI : MonoBehaviour
    {
        [SerializeField] public string id;
        
        public Slider slider;
        public SettingsPanelUI panel;
        public void Start()
        {
            panel = GetComponentInParent<SettingsPanelUI>();
            slider = GetComponent<Slider>();
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        public void OnDestroy() => slider.onValueChanged.RemoveListener(OnValueChanged);

        public void OnValueChanged(float _)
        {
            if (panel)
                panel.InvokeSettingChanged(id, slider.value);
        }
    }
}