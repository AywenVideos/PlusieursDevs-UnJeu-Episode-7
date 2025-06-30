using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPCAssignmentUI : MonoBehaviour
{
    public static NPCAssignmentUI Instance { get; private set; }

    [SerializeField] private GameObject assignmentPanel;
    [SerializeField] private Transform npcListContent;
    [SerializeField] private GameObject npcItemPrefab;
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI assignedCountText;

    private BuildingInstance currentBuilding;
    private List<NPCListItem> npcItems = new();

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void ShowForBuilding(BuildingInstance building)
    {
        if (building == null)
            return;

        currentBuilding = building;
        buildingNameText.text = building.TileSO.tileName;
        
        // Afficher le nombre de NPC assignés
        UpdateAssignedCount();
        
        // Remplir la liste des NPC disponibles
        RefreshNPCList();
        
        // Afficher le panneau
        assignmentPanel.SetActive(true);
    }

    public void Hide()
    {
        assignmentPanel.SetActive(false);
        currentBuilding = null;
        ClearNPCList();
    }

    public void RefreshNPCList()
    {
        ClearNPCList();

        if (currentBuilding == null)
            return;

        // Vérifier le type de bâtiment pour déterminer le système d'affectation
        GoldMine goldMine = currentBuilding.GetComponent<GoldMine>();
        MilkFactory milkFactory = currentBuilding.GetComponent<MilkFactory>();
        TownHall townHall = currentBuilding.GetComponent<TownHall>();

        foreach (NPC npc in NPC.All)
        {
            // Ne montrer que les villageois (sans emploi)
            if (npc.GetJobType() != NPCType.Villager)
                continue;

            GameObject itemGO = Instantiate(npcItemPrefab, npcListContent);
            NPCListItem item = itemGO.GetComponent<NPCListItem>();
            
            if (item != null)
            {
                item.Initialize(npc, this);
                npcItems.Add(item);
            }
        }

        if (npcItems.Count == 0)
        {
            // Si aucun villageois disponible, ajouter un message
            GameObject itemGO = Instantiate(npcItemPrefab, npcListContent);
            TextMeshProUGUI text = itemGO.GetComponentInChildren<TextMeshProUGUI>();
            Button button = itemGO.GetComponentInChildren<Button>();
            
            if (text != null)
                text.text = "Aucun villageois disponible";
                
            if (button != null)
                button.interactable = false;
        }
    }

    private void ClearNPCList()
    {
        foreach (Transform child in npcListContent)
        {
            Destroy(child.gameObject);
        }
        npcItems.Clear();
    }

    public void AssignNPCToBuilding(NPC npc)
    {
        if (npc == null || currentBuilding == null)
            return;

        bool success = false;

        // Gérer l'affectation selon le type de bâtiment
        GoldMine goldMine = currentBuilding.GetComponent<GoldMine>();
        if (goldMine != null && !goldMine.assignedNPCs.Contains(npc) && goldMine.assignedNPCs.Count < goldMine.maxNPCs)
        {
            goldMine.assignedNPCs.Add(npc);
            npc.SetJobType(NPCType.Miner);
            npc.SetWorkplace(goldMine.transform);
            success = true;
        }

        MilkFactory milkFactory = currentBuilding.GetComponent<MilkFactory>();
        if (milkFactory != null && !milkFactory.assignedNPCs.Contains(npc) && milkFactory.assignedNPCs.Count < milkFactory.maxNPCs)
        {
            milkFactory.assignedNPCs.Add(npc);
            npc.SetJobType(NPCType.Milkman);
            npc.SetWorkplace(milkFactory.transform);
            success = true;
        }

        TownHall townHall = currentBuilding.GetComponent<TownHall>();
        if (townHall != null && townHall.npc == null)
        {
            townHall.npc = npc;
            npc.SetJobType(NPCType.Mayor);
            npc.SetWorkplace(townHall.transform);
            success = true;
        }

        if (success)
        {
            // Debug.Log($"[NPC] -> {npc.GetDisplayName()} a été assigné à {currentBuilding.TileSO.tileName}");
            RefreshNPCList();
            UpdateAssignedCount();
            
            // Jouer un son de succès
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySelectClip();
        }
    }

    private void UpdateAssignedCount()
    {
        int current = 0;
        int max = 0;

        GoldMine goldMine = currentBuilding.GetComponent<GoldMine>();
        if (goldMine != null)
        {
            current = goldMine.assignedNPCs.Count;
            max = goldMine.maxNPCs;
        }

        MilkFactory milkFactory = currentBuilding.GetComponent<MilkFactory>();
        if (milkFactory != null)
        {
            current = milkFactory.assignedNPCs.Count;
            max = milkFactory.maxNPCs;
        }

        TownHall townHall = currentBuilding.GetComponent<TownHall>();
        if (townHall != null)
        {
            current = townHall.npc != null ? 1 : 0;
            max = 1;
        }

        assignedCountText.text = $"Assignés: {current}/{max}";
    }
} 