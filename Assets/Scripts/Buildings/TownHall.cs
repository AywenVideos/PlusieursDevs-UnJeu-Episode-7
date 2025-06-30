using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(BuildingInstance))]
public class TownHall : MonoBehaviour
{
    public static List<TownHall> All = new();
    private GameManager gameManagerRef;
    [NonSerialized] public NPC npc;
    
    private BuildingInstance buildingInstance;

    private void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
        buildingInstance.Upgraded += Upgraded;
    }

    private void OnDisable()
    {
        All.Remove(this);
        buildingInstance.Upgraded -= Upgraded;
    }

    private void Start()
    {
        buildingInstance = GetComponent<BuildingInstance>();
        gameManagerRef = GameManager.Instance;
        npc = GetComponent<NPC>();
        StartCoroutine(GenerateResourcesCoroutine());
        gameManagerRef?.UpdateResourceLimits(); // Mettre à jour les limites dès la construction
    }
    
    private void Upgraded()
    {
        gameManagerRef?.UpdateResourceLimits();
    }

    private IEnumerator GenerateResourcesCoroutine()
    {
        while (true)
        {
            GenerateResources();
            yield return new WaitForSeconds(5f);
        }
    }

    private void GenerateResources()
    {
        if (gameManagerRef != null && gameManagerRef?.PlayerData != null)
        {
            // La production peut être ajustée en fonction du niveau d'amélioration
            int goldToGenerate = (5 + buildingInstance.CurrentLevel * 2) * gameManagerRef.NpcCount / 2; // Exemple d'ajustement
            int emeraldToGenerate = (gameManagerRef.NpcCount / 8) + buildingInstance.CurrentLevel / 2; // Exemple d'ajustement


            // Vérifier si l'ajout de l'or ne dépasse pas la limite
            int newGold = gameManagerRef.PlayerData.gold + goldToGenerate;
            if (newGold > gameManagerRef.MaxGold)
            {
                goldToGenerate = Mathf.Max(0, gameManagerRef.MaxGold - gameManagerRef.PlayerData.gold);
            }
            
            gameManagerRef.PlayerData.gold += goldToGenerate;  
            gameManagerRef.PlayerData.emerald += emeraldToGenerate;
        }
    }

    public int GetMaxGold()
    {
        return buildingInstance.CurrentLevel switch
        {
            0 => 200,
            1 => 366,
            2 => 700,
            _ => throw new NotImplementedException()
        };
    }
    
    public int GetMaxMilk()
    {
        return buildingInstance.CurrentLevel switch
        {
            0 => 200,
            1 => 350,
            2 => 600,
            _ => throw new NotImplementedException()
        };
    }
}
