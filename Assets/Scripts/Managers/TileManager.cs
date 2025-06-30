using Unity.AI.Navigation;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Entities;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-20)]
public class TileManager : MonoBehaviour
{
    #region Properties

    public static TileManager Instance { get; private set; }
    public float TileSize => tileSize;
    public TileInstance CurrentSelectedTile { get => currentSelectedTile; set => SetSelectedTile(value); }

    #endregion

    #region Variables

    [SerializeField] private int gridWidth = 50;
    [SerializeField] private int gridHeight = 50;
    [Tooltip("Tile Size in meters (2x2m)")]
    [SerializeField] private float tileSize = 2f;
    [Header("Map Generation Settings")]
    [SerializeField] private TileSO defaultGround;
    [SerializeField] private TreeSO[] trees;
    [SerializeField] private float treesProbability = 2f;

    [Header("Tree Spawning Settings")]
    [SerializeField] private float spawnInterval = 60f; // 1 minute en secondes

    private TileInstance currentSelectedTile;
    private NavMeshSurface navMeshSurface;
    private TileData[,] grid;
    private List<TreeSO> spawnableTreeSOs; // Liste des TreeSO disponibles pour la génération périodique

    #endregion

    private void Awake()
    {
        Instance = this;
        navMeshSurface = GetComponent<NavMeshSurface>();

        // Charger tous les TreeSO disponibles pour la génération périodique
        spawnableTreeSOs = TileSODatabase.GetAll().OfType<TreeSO>().ToList();
        if (spawnableTreeSOs.Count == 0)
        {
            Debug.LogWarning("TileManager: Aucun TreeSO trouvé dans TileDatabase pour la génération périodique d'arbres.");
        }
    }

    private void Start()
    {
        GenerateMap();
        StartCoroutine(SpawnTreesRoutine()); // Démarre la coroutine de génération périodique d'arbres
    }

    private void Update()
    {
        // debug pour regénérer la map
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.G))
            GenerateMap();
    }

    public void SetSelectedTile(TileInstance tileInstance)
    {

        if(currentSelectedTile != tileInstance)
        {
            if(currentSelectedTile != null)
                currentSelectedTile.OnUnSelect();

            currentSelectedTile = tileInstance;
            
            if(currentSelectedTile != null)
                currentSelectedTile.OnSelect();
            
            UserInterface.Instance.SetSelectedTileInstance(tileInstance);
        }
    }

    private void ClearMap()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    [ContextMenu("Generate")]
    public void GenerateMap(bool placeRandomTrees = true)
    {
        ClearMap();

        grid = new TileData[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for(int y = 0; y < gridHeight; y++)
            {
                PlaceTile(x, y, defaultGround);
            }
        }

        if (placeRandomTrees)
        {
            PlaceTrees(treesProbability);
        }

        // on bake la navmesh à la fin de la génération de la map
        navMeshSurface.BuildNavMesh();
    }

    public bool CheckForSpace(int x, int y, int width, int height, bool force = false)
    {
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                if (i < 0 || i >= gridWidth || j < 0 || j >= gridHeight)
                    return false;

                // ici on check juste si c'est null ou différent du ground_01 qui est le tileId 1
                if (grid[i, j] == null || grid[i, j].tileId != 1)
                {
                    if (!force) return false;
                    RemoveTile(i, j);
                    return true;
                }
            }
        }

        return true;
    }

    public void PlaceTrees(float coveringPercentage)
    {
        float probability = coveringPercentage / 100f;

        for (int x = 1; x < gridWidth-1; x++)
        {
            for (int y = 1; y < gridHeight-1; y++)
            {
                if (Random.value < probability)
                {
                    // Are you... are you.. choosing up a tree?
                    TreeSO chosenTree = trees[Random.Range(0, trees.Length)];

                    // On check si un arbre est pas trop proche d'un autre
                    if (CheckForNeighbor<TreeSO>(x, y, 3))
                    {
                        // si oui on skip ===>
                        continue;
                    }

                    ReplaceTile(x, y, chosenTree);
                }
            }
        }
    }

    private bool CheckForNeighbor(int tileId, int centerX, int centerY, int radius)
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;

                if (x == centerX && y == centerY)
                    continue;

                TileData neighbor = grid[x, y];
                if (neighbor != null && neighbor.tileId == tileId)
                    return true;
            }
        }

        return false;
    }

    private bool CheckForNeighbor<T>(int centerX, int centerY, int radius) where T : TileSO
    {
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;

                if (x == centerX && y == centerY)
                    continue;

                TileData neighbor = grid[x, y];
                if (neighbor == null || neighbor.scriptableObject == null)
                    continue;

                if (neighbor.scriptableObject is T)
                    return true;
            }
        }

        return false;
    }

    public TileInstance PlaceTile(int x, int y, TileSO tile)
    {
        TileInstance tInstance = Instantiate(tile.tilePrefab, new Vector3(tileSize * x, 0f, tileSize * y), Quaternion.identity, transform).Initialize(x, y);
        grid[x, y] = new TileData(x, y, tile.tileId, tInstance.GetDefaultData(), tInstance);
        return tInstance;
    }

    public TileData GetCurrentTile(int x, int y)
    {
        return grid[x, y];
    }

    public TileData GetFromPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.z / tileSize);
        
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return null;
        return grid[x, y];
    }
    
    public List<TileData> GetTilesInRadius(Vector3 worldPosition, int radius)
    {
        List<TileData> tilesInRange = new List<TileData>();

        int centerX = Mathf.FloorToInt(worldPosition.x / tileSize);
        int centerY = Mathf.FloorToInt(worldPosition.z / tileSize);

        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;

                float distSqr = (x - centerX) * (x - centerX) + (y - centerY) * (y - centerY);
                if (distSqr <= radius * radius)
                {
                    tilesInRange.Add(grid[x, y]);
                }
            }
        }

        return tilesInRange;
    }

    public TileInstance TryPlaceBuilding(int x, int y, TileSO tile, bool force = false)
    {
        if (!tile || !tile.tilePrefab) return null;
        int w = tile.width;
        int h = tile.height;

        if (!CheckForSpace(x, y, w, h, force))
            return null;

        Vector3 worldPos = new Vector3(x * tileSize, 0f, y * tileSize);

        TileInstance instance = Instantiate(tile.tilePrefab, worldPos, Quaternion.identity,
            transform
        ).Initialize(x, y);
        
        grid[x, y] = new TileData(x, y, tile.tileId, instance.GetDefaultData(), instance);

        for (int i = x; i < x + w; i++)
        {
            for (int j = y; j < y + h; j++)
            {
                if (i == x && j == y)
                    continue;

                grid[i, j] = new TileData(i, j, -1, "", null);
            }
        }

        return instance;
    }

    public Unit TryPlaceUnit(int x, int y, UnitSO unit)
    {
        if (!CheckForSpace(x, y, 1, 1))
            return null;

        Vector3 worldPos = new Vector3(x * tileSize, 0f, y * tileSize);

        Unit instance = Instantiate(unit.unitPrefab, worldPos, Quaternion.identity,
            transform
        ).gameObject.AddComponent<Unit>().Initialize(unit);

        
        return instance;
        
    }

    public void RemoveTile(int x, int y)
    {
        if(grid[x, y].Instance != null)
        {
            Destroy(grid[x, y].Instance.gameObject);
        }
        // After removing, ensure the grid position is set back to default ground
        // This is important to allow new tiles to be placed there and avoid null references.
        PlaceTile(x, y, defaultGround); // Place default ground after removing a tile
    }

    // New overloaded ReplaceTile method to accept Vector3 world position
    public bool ReplaceTile(Vector3 worldPosition, TileSO tile)
    {
        int x = Mathf.RoundToInt(worldPosition.x / tileSize);
        int y = Mathf.RoundToInt(worldPosition.z / tileSize);

        return ReplaceTile(x, y, tile);
    }

    public bool ReplaceTile(int x, int y, TileSO tile)
    {
        RemoveTile(x, y);
        // Call PlaceTile, as it now handles adding to the grid
        PlaceTile(x, y, tile);

        return true;
    }

    public void RegisterTileInstance(TileInstance instance)
    {
        if (instance == null || instance.TileSO == null)
            return;

        grid[instance.X, instance.Y] = new TileData(instance.X, instance.Y, instance.TileSO.tileId, instance.GetDefaultData(), instance);
    }

    /// <summary>
    /// Sauvegarde la carte actuelle en JSON
    /// </summary>
    /// <param name="path">Chemin de sauvegarde personnalisé</param>
    /// <param name="fileName">Nom du fichier</param>
    public bool SaveMap(string path = null, string fileName = "map.json")
    {
        try
        {
            TileMap map = new TileMap();
            map.width = gridWidth;
            map.height = gridHeight;
            map.tiles = new TileData[gridWidth * gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    TileData td = grid[x, y];
                    map.tiles[x + y * gridWidth] = new TileData(td.x, td.y, td.tileId, td.data, null);
                }
            }

            string json = JsonUtility.ToJson(map);

            string saveDirectory = path ?? $"{Application.dataPath}/../Saves";
            string savePath = $"{saveDirectory}/{fileName}";

            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);

            File.WriteAllText(savePath, json);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public List<PlayerData.BuildingData> GetBuildingsData()
    {
        List<PlayerData.BuildingData> buildingsData = new();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                TileData tile = grid[x, y];
                // Check if the tile exists, has an instance, and that instance has a BuildingInstance component
                if (tile != null && tile.Instance != null && tile.Instance.TryGetComponent(out BuildingInstance buildingInstance))
                {
                    buildingsData.Add(new PlayerData.BuildingData
                    {
                        type = buildingInstance.TileSO.slug,
                        position = buildingInstance.transform.position,
                        level = buildingInstance.CurrentLevel
                    });
                }
            }
        }
        return buildingsData;
    }

    public List<PlayerData.TreeData> GetTreesData()
    {
        List<PlayerData.TreeData> treesData = new();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                TileData tile = grid[x, y];
                if (tile != null && tile.Instance != null && tile.Instance.TileSO is TreeSO)
                {
                    treesData.Add(new PlayerData.TreeData
                    {
                        type = tile.Instance.TileSO.slug,
                        position = tile.Instance.transform.position
                    });
                }
            }
        }
        return treesData;
    }

    public void LoadBuildingsData(List<PlayerData.BuildingData> savedBuildingsData)
    {
        // Clear existing buildings and re-generate map without random trees
        GenerateMap(false);

        foreach (var buildingData in savedBuildingsData)
        {
            TileSO tileSO = TileSODatabase.GetBySlug(buildingData.type);
            if (tileSO != null)
            {
                // Calculate grid position from world position
                int x = Mathf.RoundToInt(buildingData.position.x / tileSize);
                int y = Mathf.RoundToInt(buildingData.position.z / tileSize);

                TileInstance instance = TryPlaceBuilding(x, y, tileSO);
                if (instance != null && instance.TryGetComponent<BuildingInstance>(out var buildingInstance))
                {
                    buildingInstance.SetLevel(buildingData.level);
                }
            }
        }
        navMeshSurface.BuildNavMesh();
    }

    public void LoadTreesData(List<PlayerData.TreeData> savedTreesData)
    {
        foreach (var treeData in savedTreesData)
        {
            TileSO treeSO = TileSODatabase.GetBySlug(treeData.type);
            if (treeSO != null)
            {
                // Calculate grid position from world position
                int x = Mathf.RoundToInt(treeData.position.x / tileSize);
                int y = Mathf.RoundToInt(treeData.position.z / tileSize);

                ReplaceTile(x, y, treeSO);
            }
        }
        navMeshSurface.BuildNavMesh();
    }

    private IEnumerator SpawnTreesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnRandomTreePeriodically();
        }
    }

    private void SpawnRandomTreePeriodically()
    {
        if (spawnableTreeSOs == null || spawnableTreeSOs.Count == 0)
        {
            Debug.LogWarning("TileManager: Aucun TreeSO disponible pour la génération périodique d'arbres.");
            return;
        }

        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Vérifier si la tuile est le terrain par défaut et n'a pas de voisins arbres ou bâtiments proches
                if (grid[x, y].tileId == defaultGround.tileId &&
                    !CheckForNeighbor<TreeSO>(x, y, 3) && // Éviter de spawner trop près d'un autre arbre
                    !CheckForNeighbor<BuildingSO>(x, y, 3)) // Éviter de spawner trop près d'un bâtiment
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        if (availablePositions.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int spawnGridPos = availablePositions[randomIndex];

            TreeSO randomTreeSO = spawnableTreeSOs[Random.Range(0, spawnableTreeSOs.Count)];
            ReplaceTile(spawnGridPos.x, spawnGridPos.y, randomTreeSO);
        }
        else
        {
            Debug.LogWarning("TileManager: Impossible de trouver un emplacement valide pour générer un arbre périodiquement.");
        }
    }
}

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public int tileId;
    public string data; // données sauvegardées du tile

    [System.NonSerialized] public TileSO scriptableObject; // Référence au scriptable object
    [System.NonSerialized] public TileInstance Instance; // Référence à l'instance du GameObject

    public TileData() { }

    public TileData(int x, int y, int tileId, string data, TileInstance instance)
    {
        this.x = x;
        this.y = y;
        this.tileId = tileId;
        this.data = data;
        this.Instance = instance;
        this.scriptableObject = TileSODatabase.GetById(tileId); // Load the scriptable object here
    }
}

[System.Serializable]
public class TileMap
{
    public int width;
    public int height;
    public TileData[] tiles;
}