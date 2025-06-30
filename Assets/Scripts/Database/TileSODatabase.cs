using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class TileSODatabase
{
    private static List<TileSO> _items;
    
    public static TileSO GetById(int id)
    {
        _items ??= LoadAllItems();
        return _items.FirstOrDefault(item => item.tileId == id);
    }
    
    public static TileSO GetBySlug(string slug)
    {
        _items ??= LoadAllItems();
        return _items.FirstOrDefault(item => item.slug == slug);
    }

    public static List<TileSO> GetAll()
    {
        _items ??= LoadAllItems();
        return _items;
    }

    private static List<TileSO> LoadAllItems() => AssetDatabase.FindAssets($"t:{nameof(TileSO)}")
        .Select(AssetDatabase.GUIDToAssetPath)
        .Select(AssetDatabase.LoadAssetAtPath<TileSO>)
        .Where(item => item != null)
        .ToList();
}