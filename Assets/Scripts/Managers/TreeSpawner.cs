using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    public static TreeSpawner Instance { get; private set; }

    [SerializeField] private List<GameObject> treePrefabs; // Assignez vos préfabs d'arbres ici dans l'inspecteur Unity
    public float spawnInterval = 60f; // 1 minutes en secondes

    private TileManager tileManager; // Référence au TileManager

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Garde le spawner actif entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        tileManager = FindAnyObjectByType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("TreeSpawner: TileManager non trouvé dans la scène. La génération d'arbres ne fonctionnera pas.");
            return;
        }
        StartCoroutine(SpawnTreesRoutine());
    }

    private IEnumerator SpawnTreesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnRandomTree();
        }
    }

    private void SpawnRandomTree()
    {
        if (treePrefabs == null || treePrefabs.Count == 0)
        {
            Debug.LogWarning("TreeSpawner: Aucun préfab d'arbre assigné. Impossible de générer un arbre.");
            return;
        }

        // Trouver un emplacement libre aléatoire
        Vector3 spawnPosition = Vector3.zero; // Valeur par défaut
        bool foundValidPosition = false;

        // Essayer de trouver un emplacement valide 50 fois
        for (int i = 0; i < 50; i++)
        {
            // Générer des coordonnées aléatoires dans les limites de la carte
            // Ces limites devront être ajustées en fonction de la taille réelle de votre carte
            // Par exemple, si votre carte est de -50 à 50 en X et Z
            float randomX = Random.Range(-50f, 50f);
            float randomZ = Random.Range(-50f, 50f);
            spawnPosition = new Vector3(randomX, 0f, randomZ); // Assurez-vous que le Y est correct pour votre terrain

            // Vérifier si la position est libre (nécessite une méthode dans TileManager ou une logique de collision)
            // Pour l'instant, nous allons simuler une position libre ou laisser Unity gérer les collisions si le prefab a un collider
            // Une meilleure approche serait de demander au TileManager une position libre
            // Exemple simplifié : Vérifier s'il n'y a pas déjà un bâtiment ou un arbre à cette position
            // Cela nécessiterait des colliders sur les bâtiments/arbres et une couche spécifique pour la détection.

            // Pour une solution rapide, nous allons juste vérifier si la position est sur le terrain et n'est pas bloquée par un collider
            RaycastHit hit;
            // Assurez-vous que le layer du terrain est "Ground" ou similaire
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f, LayerMask.GetMask("Ground")))
            {
                // Ajuster la position Y à la surface du terrain
                spawnPosition.y = hit.point.y;

                // Vérifier si la position est libre d'un autre bâtiment ou arbre (approximatif, à améliorer)
                Collider[] hitColliders = Physics.OverlapSphere(spawnPosition, 2f); // Vérifier un rayon de 2 unités
                bool isOccupied = false;
                foreach (Collider col in hitColliders)
                {
                    if (col.CompareTag("Building") || col.CompareTag("Tree")) // Assurez-vous que vos bâtiments et arbres ont ces tags
                    {
                        isOccupied = true;
                        break;
                    }
                }

                if (!isOccupied)
                {
                    foundValidPosition = true;
                    break;
                }
            }
        }

        if (foundValidPosition)
        {
            GameObject randomTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Count)];
            Instantiate(randomTreePrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[TREE SPAWNER] Arbre {randomTreePrefab.name} généré à {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("TreeSpawner: Impossible de trouver un emplacement valide pour générer un arbre.");
        }
    }
} 