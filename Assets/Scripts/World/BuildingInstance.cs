using System;
using DG.Tweening;
using UnityEngine;

public class BuildingInstance : TileInstance
{
    public event Action Upgraded;
    
    [SerializeField] private GameObject[] grounds;
    private int level = 0;
    
    public int CurrentLevel => level;
    public BuildingSO BuildingSO => TileSO as BuildingSO;

    private void Start()
    {
        OnUpgrade(level);
    }

    public void SetLevel(int level)
    {
        if (level > BuildingSO.upgrades.Count) return;
        this.level = level;
        OnUpgrade(level);
    }

    public override void OnUpgrade(int level)
    {
        base.OnUpgrade(level);
        Upgraded?.Invoke();
    }

    /// <summary>
    /// Méthode pour désactiver le sol du bâtiment quand il est en mode build
    /// </summary>
    /// <param name="value"></param>
    public void SetBuildMode(bool value)
    {
        for (int i = 0; i < grounds.Length; i++)
            grounds[i].SetActive(!value);
    }

    public override void OnSelect()
    {
        base.OnSelect();

        transform.localScale = Vector3.one * 0.9f;
        transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce);
    }
}
