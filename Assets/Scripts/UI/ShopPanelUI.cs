using UnityEngine;

public class ShopPanelUI : PanelUI
{
    public static ShopPanelUI Instance { get; private set; }

    [SerializeField] private Transform content;
    [SerializeField] private ShopItemUI itemPrefab;

    public override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    public override void Start()
    {
        base.Start();
        BuildManager.Instance.OnBuildingUnlocked += (_) => ShowTilesInShop();
    }

    public override void Show()
    {
        base.Show();
        ShowTilesInShop();
    }

    private void Clear()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    public void ShowTilesInShop()
    {
        Clear();
        
        foreach (BuildingSO buildingSO in BuildingSODatabase.GetAllAvailableShop())
        {
            Instantiate(itemPrefab, content).Initialize(buildingSO);
        }
    }
}
