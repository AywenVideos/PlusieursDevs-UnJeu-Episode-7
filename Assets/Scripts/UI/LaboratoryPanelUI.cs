using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LaboratoryPanelUI : PanelUI
{
    public static LaboratoryPanelUI Instance { get; private set; }

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private LaboratoryItemUI itemPrefab;
    [SerializeField] private BuildingSO laboratoryBuildingSO;
    
    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void Show()
    {
        base.Show();
        RefreshItems();
        scrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void RefreshItems()
    {
        Clear();

        // Get all building that can be unlock on laboratory
        List<BuildingSO> buildings = BuildingSODatabase.GetAll()
            .Where(b => b.UnlockNode.Any(u => u.building == laboratoryBuildingSO))
            .OrderBy(b => !b.Unlocked)
            .ToList();
        
        foreach (BuildingSO building in buildings)
        {
            LaboratoryItemUI laboratoryItemUI = Instantiate(itemPrefab, content);
            laboratoryItemUI.Initialize(building, laboratoryBuildingSO);
        }
    }

    private void Clear()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }
}