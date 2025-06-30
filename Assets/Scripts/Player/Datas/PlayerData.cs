using System.IO;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Données du joueur, peuvent être sérialisées, chargées, etc. => pratique pour la sauvegarde
/// </summary>
[System.Serializable]
public class PlayerData
{
    // Resources
    public int gold = 500;
    public int milk = 10;
    public int emerald = 10;
    public int npcCount;

    // Game state
    public List<QuestManager.Quest> questsState = new();
    public List<BuildingData> buildings = new();
    public List<TreeData> trees = new();
    
    // Settings
    public int masterVolume = 100;
    public int musicVolume = 100;
    public int effectVolume = 100;

    [System.Serializable]
    public class BuildingData
    {
        public string type;
        public Vector3 position;
        public int level;
    }

    [System.Serializable]
    public class TreeData
    {
        public string type;
        public Vector3 position;
    }

    private static string GetSaveDirectory()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveDir))
            Directory.CreateDirectory(saveDir);
        return saveDir;
    }

    /// <summary>
    /// Sérialise en JSON les données
    /// </summary>
    /// <returns></returns>
    public string SerializeToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    /// <summary>
    /// Sauvegarde les données du joueur dans un dossier prédéfini
    /// </summary>
    /// <returns>Sauvegardé avec succès ?</returns>
    public bool Save()
    {
        try
        {
            string saveDirectory = GetSaveDirectory();
            string savePath = Path.Combine(saveDirectory, "playerSave.json");
            string json = SerializeToJson();
            File.WriteAllText(savePath, json);
            Toast.Info("Partie sauvegardée !", "Vos données ont été sauvegardées avec succès.");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            Toast.Error("Erreur de sauvegarde !", "Une erreur est survenue lors de la sauvegarde de vos données.");
            return false;
        }
    }

    public static PlayerData DeserializeFromJson(string jsonData)
    {
        return JsonUtility.FromJson<PlayerData>(jsonData);
    }

    public static PlayerData Load()
    {
        try
        {
            string saveDirectory = GetSaveDirectory();
            string savePath = Path.Combine(saveDirectory, "playerSave.json");

            if (!File.Exists(savePath))
            {
                return null;
            }

            string json = File.ReadAllText(savePath);
            return DeserializeFromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            Toast.Error("Sauvegarde corrompue !", "Les données du fichier de sauvegarde sont corrompues.");
            return null;
        }
    }
}