using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "ClashOfAywen/Tile", order = 0)]
public class TileSO : ScriptableObject
{
    public int tileId;
    public Sprite icon;
    public string tileName;
    public string slug;
    public TileInstance tilePrefab;
    public bool canBeDestroy = true;
    [Header("Shop Config")]
    public bool availableInShop;
    public int milkCost = 0;
    public int goldCost = 0;
    public int emeraldCost = 0;
    [Header("Réglage taille du tile pour les bâtiments (voir le gizmo vert)")]
    [Min(1)] public int width = 1;
    [Min(1)] public int height = 1;

    [Header("Offset du Selected Tile Text")]
    public Vector3 offsetSelectedTileText;
}