// This is a dummy comment to force recompilation.

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public PlayerData PlayerData { get; private set; }
    public int NpcCount { get; set; }
    public int MaxMilk { get; set; }
    public int MaxGold { get; set; }

    [HideInInspector] public UserInterface ui;
    private string savePath;

    public DialogueSO _tutorialDialogueSO;

    private void Awake()
    {
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "Saves", "playerSave.json");
    }

    private void Start()
    {
        ui = UserInterface.Instance;

        // Try to load saved game
        PlayerData = PlayerData.Load();
        if (PlayerData == null)
        {
            // No save found, start new game with random trees
            PlayerData = new PlayerData();
            TileManager.Instance.GenerateMap(true); // true = place random trees
        }
        else
        {
            // Load saved game state
            LoadGameState();
        }

        UpdateResourceLimits();

        DialogSystemUI.Instance.BeginDialogue(_tutorialDialogueSO);
        
    }

    public void UpdateResourceLimits()
    {
        TownHall townHall = FindFirstObjectByType<TownHall>();
        int bonusMaxGold = townHall?.GetMaxGold() ?? 0;
        int bonusMaxMilk = townHall?.GetMaxMilk() ?? 0;
        MaxGold = 300 + bonusMaxGold;
        MaxMilk = 100 + bonusMaxMilk;
    }

    private void Update()
    {
        UpdateResourcesUI();
        UpdateResourceLimits(); // Update limits every frame in case town hall is upgraded

        NpcCount = NPC.All.Count;

        // Save game with Shift+S
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }

        // Restart game with Shift+R
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
        //Cheat
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.R))
        {
            PlayerData.gold = 1000;
            PlayerData.milk = 1000;

        }
    }

    private void RestartGame()
    {
        // Delete save file if it exists
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadGameState()
    {
        // Load resources
        PlayerData.gold = PlayerData.gold;
        PlayerData.milk = PlayerData.milk;
        PlayerData.emerald = PlayerData.emerald;
        NpcCount = PlayerData.npcCount;

        // Load quests state
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.LoadQuestsState(PlayerData.questsState);
        }

        // Load buildings and trees
        if (TileManager.Instance != null)
        {
            // First load buildings (this will generate the map without random trees)
            TileManager.Instance.LoadBuildingsData(PlayerData.buildings);
            // Then load saved trees
            TileManager.Instance.LoadTreesData(PlayerData.trees);
        }
        if (AudioManager.Instance)
        {
            AudioManager.Instance.masterVolume = PlayerData.masterVolume;
            AudioManager.Instance.musicVolume = PlayerData.musicVolume;
            AudioManager.Instance.effectVolume = PlayerData.effectVolume;
            AudioManager.Instance.CalculSoundsVolume();
            SettingsPanelUI.Instance?.SetupSettings(PlayerData);
        }
    }

    /// <summary>
    /// Sauvegarde les données du joueur et la carte actuelle
    /// </summary>
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        if (PlayerData == null)
            PlayerData = new PlayerData();

        // Save resources
        PlayerData.gold = PlayerData.gold;
        PlayerData.milk = PlayerData.milk;
        PlayerData.emerald = PlayerData.emerald;
        PlayerData.npcCount = NpcCount;

        // Save quests state
        if (QuestManager.Instance != null)
        {
            PlayerData.questsState = QuestManager.Instance.GetQuestsState();
        }

        // Save buildings
        if (TileManager.Instance != null)
        {
            PlayerData.buildings = TileManager.Instance.GetBuildingsData();
        }

        // Save trees
        if (TileManager.Instance != null)
        {
            PlayerData.trees = TileManager.Instance.GetTreesData();
        }

        // Save to file
        PlayerData.Save();
    }

    public void ToggleCombatMode()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name == "01_Game" ? "03_Combat" : "01_Game");
    }

    private void UpdateResourcesUI()
    {
        if (PlayerData == null)
            return;

        ui.NPCResource.SetResource(NpcCount);
        ui.MilkResource.SetResource(PlayerData.milk, MaxMilk);
        ui.GoldResource.SetResource(PlayerData.gold, MaxGold);
        ui.EmeraldResource.SetResource(PlayerData.emerald);
    }
}