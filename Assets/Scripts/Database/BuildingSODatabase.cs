using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class BuildingSODatabase
{
    private static List<BuildingSO> _items;
    
    public static BuildingSO GetById(int id)
    {
        _items ??= LoadAllItems();
        return _items.FirstOrDefault(item => item.tileId == id);
    }

    public static List<BuildingSO> GetAll()
    {
        _items ??= LoadAllItems();
        return _items;
    }
    
    public static List<BuildingSO> GetAllAvailableShop()
    {
        _items ??= LoadAllItems();
        return _items.Where(i => i.availableInShop && i.Unlocked).ToList();
    }

    private static List<BuildingSO> LoadAllItems() => AssetDatabase.FindAssets($"t:{nameof(BuildingSO)}")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<BuildingSO>)
        .Where(item => item != null)
        .ToList();
}