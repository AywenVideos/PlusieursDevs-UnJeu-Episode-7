using System.Collections.Generic;
using UnityEngine;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<Quest> OnQuestChanged;

    [Serializable]
    public class Quest
    {
        public string questId;
        public string title;
        public string description;
        public int emeraldReward;
        public bool isCompleted;
    }

    [SerializeField] public List<Quest> quests = new List<Quest>();

    // build
    public const string QUEST_BUILD_GOLDMINE = "build_goldmine";
    [SerializeField] DialogueSO QUEST_BUILD_GOLDMINE_DIALOGUE;
    public const string QUEST_BUILD_HOUSE = "build_house";
    [SerializeField] DialogueSO QUEST_BUILD_HOUSE_DIALOGUE;
    public const string QUEST_BUILD_5_HOUSES = "build_5_houses";
    public const string QUEST_BUILD_MILKFACTORY = "build_milkfactory";
    [SerializeField] DialogueSO QUEST_BUILD_MILKFACTORY_DIALOGUE;
    public const string QUEST_BUILD_TOWNHALL  = "build_townhall";
    [SerializeField] DialogueSO QUEST_BUILD_TOWNHALL_DIALOGUE;
    public const string QUEST_BUILD_CENTER = "build_center";
    [SerializeField] DialogueSO QUEST_BUILD_CENTER_DIALOGUE;

    // funny
    public const string QUEST_CUT_TREE = "cut_tree";

    // upgrade
    public const string QUEST_UPGRADE_BUILDING = "upgrade_building";
    public const string QUEST_UPGRADE_HOUSE = "upgrade_house";
    [SerializeField] DialogueSO QUEST_UPGRADE_HOUSE_DIALOGUE;
    public const string QUEST_UPGRADE_TOWNHALL = "upgrade_townhall";

    // boss
    public const string QUEST_UPGRADE_TOWNHALL_MAX = "upgrade_townhall_max";
    public const string QUEST_UPGRADE_HOUSE_MAX = "upgrade_house_max";
    public const string QUEST_REACH_100_VILLAGERS = "reach_100_villagers";
    public const string QUEST_MAX_RESOURCES = "max_ressources";
    
    // laboratory
    public const string QUEST_UNLOCK_MILKFACTORY = "unlock_milkfactory";
    public const string QUEST_UNLOCK_CENTER = "unlock_center";

    private int houseCount = 0;

    private void Awake()
    {
        Instance = this;
        InitializeQuests();
    }

    private void Start()
    {
        CheckAndShowActiveQuest();
        BuildManager.Instance.OnBuildingUnlocked += OnBuildingUnlocked;
    }

    private void Update()
    {
        // Check for 100 villagers quest
        if (GameManager.Instance != null && GameManager.Instance.NpcCount >= 100 && !IsQuestCompleted(QUEST_REACH_100_VILLAGERS))
        {
            CompleteQuest(QUEST_REACH_100_VILLAGERS);
        }
        // Check for max resources quest
        if (GameManager.Instance != null && GameManager.Instance.PlayerData != null &&
            GameManager.Instance.PlayerData.gold >= 1000 &&
            GameManager.Instance.PlayerData.milk >= 1000 &&
            !IsQuestCompleted(QUEST_MAX_RESOURCES))
        {
            CompleteQuest(QUEST_MAX_RESOURCES);
        }
    }

    private void InitializeQuests()
    {
        quests.Clear();
        
        quests.Add(new Quest
        {
            questId = QUEST_BUILD_GOLDMINE,
            title = "L'Or des Fous",
            description = "Construisez une mine d'or pour commencer à générer des richesses",
            emeraldReward = 5,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_BUILD_HOUSE,
            title = "Un Foyer Confortable",
            description = "Construisez votre première maison",
            emeraldReward = 5,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_BUILD_MILKFACTORY,
            title = "Lait d'Or",
            description = "Construisez une usine à lait pour produire du lait",
            emeraldReward = 5,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_BUILD_TOWNHALL,
            title = "El famoso HDV",
            description = "M'enfin voyons... nous devons construire une mairie\nAYWEN PRESIDENT",
            emeraldReward = 10,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_CUT_TREE,
            title = "Marre des Arbres !",
            description = "Clique sur un arbre puis sur la poubelle pour le détruire\n100% écologique",
            emeraldReward = 5,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_UPGRADE_BUILDING,
            title = "L'Excellence",
            description = "Améliorez un bâtiment pour augmenter sa production",
            emeraldReward = 20,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_UPGRADE_HOUSE,
            title = "Une Grande Maison",
            description = "Améliorez une maison pour accueillir une famille nombreuse.\nParce que les enfants, ça grandit... et ça se multiplie !",
            emeraldReward = 20,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_UPGRADE_TOWNHALL,
            title = "Le Coeur du Royaume",
            description = "Améliorez l'Hôtel de Ville pour renforcer votre domaine",
            emeraldReward = 30,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_BUILD_5_HOUSES,
            title = "Un Toit pour Tous",
            description = "Construisez 5 maisons pour loger tous les SDF du coin\nLa rue n'est pas un foyer !",
            emeraldReward = 30,
            isCompleted = false
        });


        quests.Add(new Quest
        {
            questId = QUEST_REACH_100_VILLAGERS,
            title = "La Grande Ville",
            description = "Atteignez 100 villageois dans votre ville.\nUne ville sans habitants n'est qu'un décor !",
            emeraldReward = 40,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_UPGRADE_TOWNHALL_MAX,
            title = "Le Pacte avec le Diable",
            description = "Améliorez l'Hôtel de Ville au niveau maximum (3).\n Le diable est dans les détails !",
            emeraldReward = 50,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_MAX_RESOURCES,
            title = "Compte Blindé",
            description = "Atteignez 1k d'or et 1k de lait\nEsprit Kaizen",
            emeraldReward = 60,
            isCompleted = false
        });

        quests.Add(new Quest
        {
            questId = QUEST_UPGRADE_HOUSE_MAX,
            title = "Bienvenue chez nous !",
            description = "Améliorez au max une maison ! Plus d'ouvrier ducoup eheh !",
            emeraldReward = 20,
            isCompleted = false
        });

        quests.Add(new Quest 
        { 
            questId = QUEST_BUILD_CENTER,
            title = "Investissons !",
            description = "Construisez un endroit pour investir",
            emeraldReward = 30,
            isCompleted = false
        });
        
        quests.Add(new Quest 
        { 
            questId = QUEST_UNLOCK_MILKFACTORY,
            title = "Lait’s go!",
            description = "Perce le secret du lait et ne le dis à personne.",
            emeraldReward = 10,
            isCompleted = false
        });
        
        quests.Add(new Quest 
        { 
            questId = QUEST_UNLOCK_CENTER,
            title = "En avant vers la faillite!",
            description = "Débloque le centre d’investissement.\nTu vas probablement tout perdre avec panache.",
            emeraldReward = 10,
            isCompleted = false
        });

        // Clear any saved quest states at game start
        // PlayerPrefs.DeleteAll(); // Removed as GameManager handles saving/loading
        // PlayerPrefs.Save(); // Removed as GameManager handles saving/loading
    }

    public List<Quest> GetQuestsState()
    {
        return quests;
    }

    public void LoadQuestsState(List<Quest> savedQuestsState)
    {
        foreach (var savedQuest in savedQuestsState)
        {
            Quest existingQuest = quests.Find(q => q.questId == savedQuest.questId);
            if (existingQuest != null)
            {
                existingQuest.isCompleted = savedQuest.isCompleted;
            }
            else
            {
                Debug.LogWarning($"Quest with ID '{savedQuest.questId}' found in save data but not in current quest definitions. Skipping.");
            }
        }
    }

    private void OnBuildingUnlocked(BuildingSO building)
    {
        if (building.slug == "milkfactory_01")
        {
            if (!IsQuestCompleted(QUEST_UNLOCK_MILKFACTORY))
            {
                CompleteQuest(QUEST_UNLOCK_MILKFACTORY);
            }
        }
        else if (building.slug == "center_01")
        {
            if (!IsQuestCompleted(QUEST_UNLOCK_CENTER))
            {
                CompleteQuest(QUEST_UNLOCK_CENTER);
            }
        }
    }
    
    public void OnBuildingPlaced(BuildingInstance building)
    {
        if (building == null) return;

        string buildingSlug = building.TileSO.slug.ToLower();
        
        if (buildingSlug.Contains("house"))
        {
            houseCount++;
            if (!IsQuestCompleted(QUEST_BUILD_HOUSE))
            {
                CompleteQuest(QUEST_BUILD_HOUSE);
                LaunchDialogueFromQuest(QUEST_BUILD_HOUSE_DIALOGUE);
            }
            if (houseCount >= 5 && !IsQuestCompleted(QUEST_BUILD_5_HOUSES))
            {
                CompleteQuest(QUEST_BUILD_5_HOUSES);
            }
        }
        else if (buildingSlug.Contains("goldmine") && !IsQuestCompleted(QUEST_BUILD_GOLDMINE))
        {
            CompleteQuest(QUEST_BUILD_GOLDMINE);
            LaunchDialogueFromQuest(QUEST_BUILD_GOLDMINE_DIALOGUE);
        }
        else if (buildingSlug.Contains("milkfactory") && !IsQuestCompleted(QUEST_BUILD_MILKFACTORY))
        {
            CompleteQuest(QUEST_BUILD_MILKFACTORY);
            LaunchDialogueFromQuest(QUEST_BUILD_MILKFACTORY_DIALOGUE);
        }
        else if (buildingSlug.Contains("townhall") && !IsQuestCompleted(QUEST_BUILD_TOWNHALL))
        {
            CompleteQuest(QUEST_BUILD_TOWNHALL);
            LaunchDialogueFromQuest(QUEST_BUILD_TOWNHALL_DIALOGUE);

        } else if(buildingSlug.Contains("center") && !IsQuestCompleted(QUEST_BUILD_CENTER))
        {
            CompleteQuest(QUEST_BUILD_CENTER);
            LaunchDialogueFromQuest(QUEST_BUILD_CENTER_DIALOGUE);
        }
    }

    public void OnBuildDestroyed(TileInstance tileInst)
    {
        if (tileInst == null) return;

        string buildingSlug = tileInst.TileSO.slug.ToLower();

        if(buildingSlug.Contains("tree"))
        {
            if(!IsQuestCompleted(QUEST_CUT_TREE))
            {
                CompleteQuest(QUEST_CUT_TREE);
            }
        }
    }

    public void OnBuildingUpgraded(BuildingInstance building)
    {
        if (building == null) return;

        TownHall townHall = building.GetComponent<TownHall>();
        House house = building.GetComponent<House>();
        
        if (building.CurrentLevel > 0)
        {
            if (!IsQuestCompleted(QUEST_UPGRADE_BUILDING))
            {
                CompleteQuest(QUEST_UPGRADE_BUILDING);
            }

            if (townHall != null)
            {
                if (!IsQuestCompleted(QUEST_UPGRADE_TOWNHALL))
                {
                    CompleteQuest(QUEST_UPGRADE_TOWNHALL);
                }
                
                if (building.CurrentLevel >= building.BuildingSO.upgrades.Count)
                {
                    if (!IsQuestCompleted(QUEST_UPGRADE_TOWNHALL_MAX))
                    {
                        CompleteQuest(QUEST_UPGRADE_TOWNHALL_MAX);
                    }
                }
            }
            
            if (house != null)
            {
                if (!IsQuestCompleted(QUEST_UPGRADE_HOUSE))
                {
                    CompleteQuest(QUEST_UPGRADE_HOUSE);
                }
                
                if (building.CurrentLevel >= building.BuildingSO.upgrades.Count)
                {
                    if (!IsQuestCompleted(QUEST_UPGRADE_HOUSE_MAX))
                    {
                        CompleteQuest(QUEST_UPGRADE_HOUSE_MAX);
                    }
                }
            }
        }
    }

    public void CompleteQuest(string questId)
    {
        Quest quest = quests.Find(q => q.questId == questId && !q.isCompleted);
        if (quest != null)
        {
            quest.isCompleted = true;
            
            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                GameManager.Instance.PlayerData.emerald += quest.emeraldReward;
                // Show completion message only when completing a quest
                Toast.Message("Quête accomplie!", $"{quest.title}: {quest.emeraldReward} émeraudes obtenues!");
            }
            
            // Only invoke OnQuestChanged, don't show active quest message here
            OnQuestChanged?.Invoke(GetActiveQuest());
        }
    }

    public void LaunchDialogueFromQuest(DialogueSO SO)
    {
        DialogSystemUI.Instance.BeginDialogue(SO);
    }

    private bool IsQuestCompleted(string questId)
    {
        Quest quest = quests.Find(q => q.questId == questId);
        return quest != null && quest.isCompleted;
    }

    private void CheckAndShowActiveQuest()
    {
        Quest activeQuest = GetActiveQuest();
        
        if (activeQuest != null)
        {
            // Don't show a popup here, just update the UI
            OnQuestChanged?.Invoke(activeQuest);
        }
        else
        {
            // Show completion message only when all quests are done
            Toast.Message("Félicitations!", "Vous avez complété toutes les tâches, bien joué ! Maintenant, amusez-vous !");
            OnQuestChanged?.Invoke(null);
        }
    }
    
    public void NotifyBuildingUpgraded(BuildingInstance building)
    {
        OnBuildingUpgraded(building);
    }
    
    public Quest GetActiveQuest()
    {
        return quests.Find(q => !q.isCompleted);
    }

    public List<Quest> GetAllQuests()
    {
        return quests;
    }
} 