using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;

    private UnitSO unitSo;

    public void Initialize(UnitSO unitSo)
    {
        this.unitSo = unitSo;
        
        text.text = unitSo.unitName;
        icon.sprite = unitSo.icon;
    }

    public void Select()
    {
        UnitPanelUI.Instance.SelectUnit(unitSo);
    }
}