using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitPanelUI : PanelUI
{
    public static UnitPanelUI Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private UnitItemUI itemPrefab;
    public List<UnitSO> units;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void Start()
    {
        base.Start();
        BuildManager.Instance.OnBuildingUnlocked += (_) => ShowUnitsInUnitsSelector();
    }

    public override void Show()
    {
        base.Show();
        ShowUnitsInUnitsSelector();
    }

    private void Clear()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    public void SelectUnit(UnitSO unit)
    {
        BuildManager.Instance.HoldUnit(unit);
    }

    public void ShowUnitsInUnitsSelector()
    {
        Clear();
        
        foreach (UnitSO unitSo in units.Where(x => !x.isEnemy))
        {
            Instantiate(itemPrefab, content).Initialize(unitSo);
        }
    }
}
