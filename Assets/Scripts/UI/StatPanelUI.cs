using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StatsPanelUI : MonoBehaviour
{
    public static StatsPanelUI Instance { get; private set; }
    
    [SerializeField] private GameObject panelObject;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI productionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button assignWorkersButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject costPanel;
    [SerializeField] private TextMeshProUGUI goldCostText;
    [SerializeField] private TextMeshProUGUI milkCostText;

    private BuildingInstance currentBuilding;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            panelObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (panelObject.activeSelf && currentBuilding != null)
        {
            UpdateProductionInfo(currentBuilding);
            UpdateUpgradeButton(currentBuilding);
            UpdateLevelText(currentBuilding);
        }
    }

    public void ShowForBuilding(BuildingInstance building)
    {
        if (building == null)
            return;

        currentBuilding = building;
        titleText.text = building.TileSO.tileName;
        BuildingSO buildingSO = building.BuildingSO;
        
        if (buildingSO != null)
        {
            // Afficher la vie du bâtiment
            contentText.text = $"Health: {buildingSO.health}";
            
            // Afficher la production actuelle selon le type de bâtiment
            UpdateProductionInfo(building);
            
            // Mettre à jour l'affichage du bouton d'amélioration
            UpdateUpgradeButton(building);
            
            // Afficher le niveau actuel du bâtiment
            UpdateLevelText(building);
        }
        else
        {
            contentText.text = string.Empty;
            productionText.text = string.Empty;
            levelText.text = string.Empty;
        }
        
        // Afficher le bouton d'affectation uniquement pour les bâtiments qui acceptent des travailleurs
        bool canAssignWorkers = building.GetComponent<GoldMine>() != null || 
                               building.GetComponent<MilkFactory>() != null || 
                               building.GetComponent<TownHall>() != null;
                               
        if (assignWorkersButton != null)
            assignWorkersButton.gameObject.SetActive(canAssignWorkers);
            
        Show();
    }
    
    private void UpdateLevelText(BuildingInstance building)
    {
        if (levelText == null)
            return;

        string message = $"Niveau: <color=red>{building.CurrentLevel + 1}</color>";
        if (building.CurrentLevel >= building.BuildingSO.upgrades.Count)
        {
            message += $" <color=red>(MAX)</color>";
        }
        levelText.text = message;
    }
    
    private void UpdateProductionInfo(BuildingInstance building)
    {
        if (productionText == null)
            return;

        string productionInfo = "";

        if (building.TryGetComponent(out GoldMine goldMine))
        {
            float currentProduction = goldMine.GoldPerMinute * GetProductionMultiplier(building);
            productionInfo = $"Production: {currentProduction:F1} Or/min";
        }

        if (building.TryGetComponent(out Center center))
        {
            float currentProduction = center.GoldPerMinute * GetProductionMultiplier(building);
            productionInfo = $"Production: {currentProduction:F1} Or/min avec un risque de {center.risk}%";
        }
        
        if (building.TryGetComponent(out MilkFactory milkFactory))
        {
            float currentProduction = milkFactory.MilkPerMinute * GetProductionMultiplier(building);
            productionInfo = $"Production: {currentProduction:F1} Lait/min";
        }
        
        if (building.TryGetComponent(out TownHall townHall))
        {
            // Afficher une information de production pour l'Hôtel de Ville
            // Par exemple, l'impact sur la génération de ressources par villageois
            productionInfo = $"Génère des ressources pour {GameManager.Instance.NpcCount} villageois. Efficacité: {GetProductionMultiplier(building):F1}x";
        }

        if (building.TryGetComponent(out House house))
        {
            productionInfo = $"Capacité d'accueil: {house.CurrentMaxOccupants} villageois";
        }
        
        productionText.text = productionInfo;
    }
    
    private void UpdateUpgradeButton(BuildingInstance building)
    {
        if (building.CurrentLevel >= building.BuildingSO.upgrades.Count)
        {
            upgradeButton.interactable = false;
            costPanel.SetActive(false);
        }
        else
        {
            UpgradeNode upgrade = building.BuildingSO.upgrades[building.CurrentLevel];
            upgradeButton.interactable = true;
            costPanel.SetActive(true);
            goldCostText.text = upgrade.goldCost.ToString();
            milkCostText.text = upgrade.milkCost.ToString();
        }
        upgradeButton.gameObject.SetActive(building.BuildingSO.upgrades.Count > 0);
    }
    
    private float GetProductionMultiplier(BuildingInstance building)
    {
        if (building.CurrentLevel == 0) return 1;
        if (building.CurrentLevel - 1 < building.BuildingSO.upgrades.Count)
        {
            return building.BuildingSO.upgrades[building.CurrentLevel - 1].productionMultiplier;
        }
        return 1;
    }
    
    // Fonction pour améliorer le bâtiment actuel (appelée par le bouton d'amélioration)
    public void UpgradeCurrentBuilding()
    {
        if (currentBuilding == null)
        {
            Debug.LogError("UpgradeCurrentBuilding: currentBuilding est nul. Impossible de procéder à l'amélioration.");
            return;
        }
        
        bool upgradeSuccessful = false;
        
        // Vérifier si ce n'est pas le niveau max
        if (currentBuilding.CurrentLevel <= currentBuilding.BuildingSO.upgrades.Count)
        {
            int goldCost = currentBuilding.BuildingSO.upgrades[currentBuilding.CurrentLevel].goldCost;
            int milkCost = currentBuilding.BuildingSO.upgrades[currentBuilding.CurrentLevel].milkCost;
            
            if (GameManager.Instance != null && GameManager.Instance.PlayerData.gold >= goldCost && GameManager.Instance.PlayerData.milk >= milkCost)
            {
                GameManager.Instance.PlayerData.gold -= goldCost;
                GameManager.Instance.PlayerData.milk -= milkCost;
                currentBuilding.SetLevel(currentBuilding.CurrentLevel + 1);
                upgradeSuccessful = true;
                
                string message = "";
                if (currentBuilding.GetComponent<Center>() != null)
                    message = $"Centre d'investissement améliorée au niveau {currentBuilding.CurrentLevel + 1}";
                if (currentBuilding.GetComponent<GoldMine>() != null)
                    message = $"Mine d'or améliorée au niveau {currentBuilding.CurrentLevel + 1}";
                if (currentBuilding.GetComponent<House>() != null)
                    message = $"Maison améliorée au niveau {currentBuilding.CurrentLevel + 1}. Plus de place pour vos villageois !";
                if (currentBuilding.GetComponent<MilkFactory>() != null)
                    message = $"Usine de lait améliorée au niveau {currentBuilding.CurrentLevel + 1}";
                if (currentBuilding.GetComponent<TownHall>() != null)
                    message = $"Hôtel de Ville amélioré au niveau {currentBuilding.CurrentLevel + 1}";
                
                Toast.Message("Amélioration réussie!", message);
            }
            else
            {
                Toast.Error("Amélioration impossible", "Vous n'avez pas les ressources nécessaires!");
            }
        }
        else
        {
            Toast.Error("Amélioration impossible", "Le batiment a déjà atteint son niveau maximum !");
        }
        
        // --- Actions post-amélioration (si une amélioration a réussi) ---
        if (upgradeSuccessful)
        {
            UpdateProductionInfo(currentBuilding);
            UpdateUpgradeButton(currentBuilding);
            UpdateLevelText(currentBuilding);
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBuildClip();

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnBuildingUpgraded(currentBuilding);
            }
        }
    }
    
    public List<NPC> GetAvailableVillagers()
    {
        List<NPC> availableVillagers = new List<NPC>();
        
        foreach (NPC npc in NPC.All)
        {
            if (npc.GetJobType() == NPCType.Villager)
            {
                availableVillagers.Add(npc);
            }
        }
        
        return availableVillagers;
    }
    
    public bool AssignVillagerToBuilding(NPC npc, BuildingInstance building)
    {
        if (npc == null || building == null || npc.GetJobType() != NPCType.Villager)
            return false;
            
        bool success = false;
        
        GoldMine goldMine = building.GetComponent<GoldMine>();
        if (goldMine != null && !goldMine.assignedNPCs.Contains(npc) && goldMine.assignedNPCs.Count < goldMine.maxNPCs)
        {
            goldMine.assignedNPCs.Add(npc);
            npc.SetJobType(NPCType.Miner);
            npc.SetWorkplace(goldMine.transform);
            success = true;
        }

        MilkFactory milkFactory = building.GetComponent<MilkFactory>();
        if (milkFactory != null && !milkFactory.assignedNPCs.Contains(npc) && milkFactory.assignedNPCs.Count < milkFactory.maxNPCs)
        {
            milkFactory.assignedNPCs.Add(npc);
            npc.SetJobType(NPCType.Milkman);
            npc.SetWorkplace(milkFactory.transform);
            success = true;
        }

        TownHall townHall = building.GetComponent<TownHall>();
        if (townHall != null && townHall.npc == null)
        {
            townHall.npc = npc;
            npc.SetJobType(NPCType.Mayor);
            npc.SetWorkplace(townHall.transform);
            success = true;
        }
        
        if (success)
        {
        }
        
        return success;
    }
    
    public BuildingInstance GetCurrentBuilding()
    {
        return currentBuilding;
    }
    
    public void Hide()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
        else
            gameObject.SetActive(false);
            
        currentBuilding = null;
    }

    public void Show()
    {
        if (panelObject != null)
            panelObject.SetActive(true);
        else
            gameObject.SetActive(true);
    }
    
    // Méthode pour fermer le panneau (à assigner au bouton Close dans l'inspecteur)
    public void ClosePanel()
    {
        Hide();
    }
}
