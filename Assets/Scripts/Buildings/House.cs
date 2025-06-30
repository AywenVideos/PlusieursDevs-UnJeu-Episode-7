using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public static List<House> All = new();

    [SerializeField] private int baseMaxOccupants = 2;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform spawnPoint;
    
    private BuildingInstance buildingInstance;
    private List<GameObject> spawnedNPCs = new();

    public int CurrentMaxOccupants => baseMaxOccupants + buildingInstance.CurrentLevel; // Chaque niveau ajoute 1 place

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
        StartCoroutine(SpawnNPCs());
    }
    
    private void OnDestroy()
    {
        // Détruit tous les NPC spawnés
        foreach (GameObject npc in spawnedNPCs)
        {
            Destroy(npc);
        }
    }
    
    private IEnumerator SpawnNPCs()
    {
        while (true)
        {
            if (spawnedNPCs.Count < CurrentMaxOccupants)
            {
                if (npcPrefab != null && spawnPoint != null)
                {
                    GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
                    spawnedNPCs.Add(newNPC);
                }
                else
                {
                    Debug.LogWarning("npcPrefab ou spawnPoint n'est pas assigné !");
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }
}