using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BuildingInstance))]
public class GoldMine : MonoBehaviour
{
    public static List<GoldMine> All = new();
    
    public int baseGold = 10;
    public float interval = 5f;
    public int maxNPCs = 3;
    
    [NonSerialized] public List<NPC> assignedNPCs = new();
    
    private BuildingInstance buildingInstance;
    
    // Calcul de la production d'or par minute
    public float GoldPerMinute => baseGold * 60f / interval;

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        buildingInstance = GetComponent<BuildingInstance>();
        StartCoroutine(GenerateGoldCoroutine());
    }

    private IEnumerator GenerateGoldCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Nettoyer la liste des NPCs détruits
            CleanupDestroyedNPCs();
            
            // Générer de l'or même sans NPCs assignés, mais avec un multiplicateur réduit
            float multiplier = buildingInstance.CurrentLevel switch
            {
                0 => 1f,
                1 => 1.5f,
                2 => 2f,
                _ => throw new NotImplementedException()
            };

            int totalGold = Mathf.RoundToInt(baseGold * multiplier);
            
            // Si des NPCs sont assignés, augmenter la production
            if (assignedNPCs.Count > 0)
            {
                totalGold *= assignedNPCs.Count;
            }
            
            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                
                // Vérifier si l'ajout de l'or ne dépasse pas la limite
                int newGold = GameManager.Instance.PlayerData.gold + totalGold;
                if (newGold > GameManager.Instance.MaxGold)
                {
                    totalGold = Mathf.Max(0, GameManager.Instance.MaxGold - GameManager.Instance.PlayerData.gold);
                }
                
                GameManager.Instance.PlayerData.gold += totalGold;
                GameManager.Instance.ui.GoldResource.UpdateAverage((int)(totalGold / interval));
            }
        }
    }

    // Méthode pour nettoyer les références nulles
    private void CleanupDestroyedNPCs()
    {
        for (int i = assignedNPCs.Count - 1; i >= 0; i--)
        {
            if (assignedNPCs[i] == null)
            {
                assignedNPCs.RemoveAt(i);
            }
        }
    }

    // Méthode publique pour retirer un NPC spécifique
    public void RemoveNPC(NPC npc)
    {
        if (assignedNPCs.Contains(npc))
        {
            assignedNPCs.Remove(npc);
        }
    }

    // Méthode pour obtenir le nombre de places disponibles
    public int GetAvailableSlots()
    {
        CleanupDestroyedNPCs();
        return maxNPCs - assignedNPCs.Count;
    }
}