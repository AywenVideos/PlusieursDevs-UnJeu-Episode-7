using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BuildingInstance))]
public class Center : MonoBehaviour
{
    private static List<Center> All = new();
    
    public int baseGold = 50;
    public float risk = 50;
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

            // G�n�rer de l'or m�me sans NPCs assign�s, mais avec un multiplicateur r�duit
            float multiplier = buildingInstance.CurrentLevel switch
            {
                0 => 1f,
                1 => 1.5f,
                2 => 2f,
                _ => throw new NotImplementedException()
            };

            int totalGold = Mathf.RoundToInt(baseGold * multiplier);

            // Si des NPCs sont assign�s, augmenter la production
            if (assignedNPCs.Count > 0)
            {
                totalGold *= assignedNPCs.Count;
            }

            if (GameManager.Instance != null && GameManager.Instance.PlayerData != null)
            {
                // V�rifier si l'ajout de l'or ne d�passe pas la limite
                int newGold = GameManager.Instance.PlayerData.gold + totalGold;
                if (newGold > GameManager.Instance.MaxGold)
                {
                    totalGold = Mathf.Max(0, GameManager.Instance.MaxGold - GameManager.Instance.PlayerData.gold);
                }

                // Le risque d'investissement
                if (UnityEngine.Random.Range(0, 100) <= risk) totalGold  = Mathf.RoundToInt(totalGold * (-0.1f));
                GameManager.Instance.PlayerData.gold += totalGold;
                GameManager.Instance.ui.GoldResource.UpdateAverage((int)(totalGold / interval));
            }
        }
    }

    // M�thode pour nettoyer les r�f�rences nulles
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

    // M�thode publique pour retirer un NPC sp�cifique
    public void RemoveNPC(NPC npc)
    {
        if (assignedNPCs.Contains(npc))
        {
            assignedNPCs.Remove(npc);
        }
    }

    // M�thode pour obtenir le nombre de places disponibles
    public int GetAvailableSlots()
    {
        CleanupDestroyedNPCs();
        return maxNPCs - assignedNPCs.Count;
    }
}
