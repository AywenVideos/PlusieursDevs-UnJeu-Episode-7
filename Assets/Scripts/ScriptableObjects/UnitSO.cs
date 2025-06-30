using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "ClashOfAywen/Unit", order = 3)]
public class UnitSO : ScriptableObject
{
    public string unitName;
    public Sprite icon;
    public int cost;
    public int health;
    public int attackDamage;
    public UnitInstance unitPrefab;
    public GameObject healthBarPrefab;
    public bool isEnemy = false;
}

