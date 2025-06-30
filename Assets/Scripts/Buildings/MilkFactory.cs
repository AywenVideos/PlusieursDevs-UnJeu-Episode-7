using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BuildingInstance))]
public class MilkFactory : MonoBehaviour
{
    public static List<MilkFactory> All = new();
    public int baseMilk = 10;
    public float interval = 5f;
    public int maxNPCs = 1;
    [NonSerialized] public List<NPC> assignedNPCs = new();
    
    private BuildingInstance buildingInstance;
    
    // Calcul de la production de lait par minute
    public float MilkPerMinute => (baseMilk * 60f / interval);

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
        StartCoroutine(GenerateMilk());
    }

    private IEnumerator GenerateMilk()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            // Nettoyer la liste des NPCs détruits
            CleanupDestroyedNPCs();
            
            // Générer du lait même sans NPCs assignés, mais avec un multiplicateur réduit
            float multiplier = buildingInstance.CurrentLevel switch
            {
                0 => 1f,
                1 => 1.5f,
                2 => 2f,
                _ => throw new NotImplementedException()
            };

            int totalMilk = Mathf.RoundToInt(baseMilk * multiplier);
            
            // Si des NPCs sont assignés, augmenter la production
            if (assignedNPCs.Count > 0)
            {
                totalMilk *= assignedNPCs.Count;
            }
            
            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                // Vérifier si l'ajout du lait ne dépasse pas la limite
                int newMilk = GameManager.Instance.PlayerData.milk + totalMilk;
                if (newMilk > GameManager.Instance.MaxMilk)
                {
                    totalMilk = Mathf.Max(0, GameManager.Instance.MaxMilk - GameManager.Instance.PlayerData.milk);
                }
                
                GameManager.Instance.PlayerData.milk += totalMilk;
                GameManager.Instance.ui.MilkResource.UpdateAverage(Mathf.RoundToInt(totalMilk / interval));
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