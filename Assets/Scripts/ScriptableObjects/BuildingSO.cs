using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "ClashOfAywen/Building", order = 1)]
public class BuildingSO : TileSO
{
    [Header("Stats")]
    public int health = 1000;
    [TextArea] public string description;
    public List<UpgradeNode> upgrades;
    public bool Unlocked;
    public List<UnlockNode> UnlockNode; 
}

[System.Serializable]
public class UpgradeNode
{
    public int goldCost;
    public int milkCost;
    public float productionMultiplier;
}

[System.Serializable]
public class UnlockNode
{
    public BuildingSO building;
    public int goldCost;
    public int milkCost;
    public int emeraldCost;
}
