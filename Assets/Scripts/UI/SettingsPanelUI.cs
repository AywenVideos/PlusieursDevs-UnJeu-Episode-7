using System;
using UI;
using UnityEngine;

public class SettingsPanelUI : PanelUI
{
    public static SettingsPanelUI Instance { get; private set; }

    [SerializeField] private Transform content;
    public event Action<string, object> OnSettingChanged;
    
    public const string SETTING_ITEM_01 = "VOLUME-MASTER";
    public const string SETTING_ITEM_02 = "VOLUME-MUSIC";
    public const string SETTING_ITEM_03 = "VOLUME-EFFECT";

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public void InvokeSettingChanged(string id, float sliderValue)
    {
        OnSettingChanged?.Invoke(id, sliderValue);
    }

    public void SetupSettings(PlayerData playerData)
    {
        foreach (var setting in GetComponentsInChildren<SettingItemUI>())
        {
            if (setting.slider)
                setting.slider.value = setting.id switch
                {
                    SETTING_ITEM_01 => playerData.masterVolume,
                    SETTING_ITEM_02 => playerData.musicVolume,
                    SETTING_ITEM_03 => playerData.effectVolume,
                    _ => setting.slider.value
                };
        }
    }
}