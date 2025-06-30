// This is a dummy comment to force recompilation.

using System;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using AttackObjects;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class CombatGameManager : MonoBehaviour
{
    public static CombatGameManager Instance { get; private set; }
    public static PlayerData PlayerData { get; private set; }

    public List<MapData> availableDatas;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TileManager.Instance.GenerateMap();
        PlayerData = PlayerData.Load();
        if (availableDatas == null || availableDatas.Count == 0)
        {
            Toast.Error("Bad configuration", "There is no enemy map data registered.");
            DestroyImmediate(this);
            return;
        }
        if (!UnitPanelUI.Instance.units.Any(x => x.isEnemy))
            Toast.Warning("No ennemies found", "There is no found ennemies in unit list - the fight will be empty !");
        
        MapData targetedMap = availableDatas[Random.Range(0, availableDatas.Count)];
        Toast.Info("Party trouvé", $"Vous allez attaquer le village de {targetedMap.mapName}");
        
        foreach (var bat in targetedMap.batDatas)
        {
            BuildingSO so = TileSODatabase.GetBySlug(bat.id) as BuildingSO;
            if (!so)
            {
                Toast.Warning("Invalid batiment", $"Un batiment enregistré n'est pas valide - {bat.id}");
                continue;
            }
            
            Debug.Log("Try placing building so " + so.tileName);
            TileInstance instance = TileManager.Instance.TryPlaceBuilding(bat.x, bat.z, so, true);
            if (instance != null && instance.TryGetComponent<BuildingInstance>(out var buildingInstance))
            {
                instance.transform.localScale = Vector3.one * 0.7f;
                instance.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);

                Debug.Log("Placed: " + instance.GetInstanceID() + "\n" + instance.gameObject.name + "\n" + instance.transform.position);

                Batiment obj = instance.gameObject.AddComponent<Batiment>();
                obj.health = so.health;
                obj.remainingHealth = so.health;
                obj.instance = instance;
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.transform.localScale = new Vector3(1f, 10f, 1f);
                primitive.transform.position = new Vector3(instance.transform.position.x, instance.transform.position.y, instance.transform.position.z);
                StartCoroutine(DelayLevel(buildingInstance, bat.level));
            }
            else
            {
                Toast.Warning("Placement failed", "Impossible de placer " + so.tileName);
                continue;
            }
        }
        
        //TODO Generate map of opponent, potentially multiplayer
        //Frrot fais toi plaisir j'ai rien fait quasiment 💀
        //Au moins essaie de finaliser les combats, un peu comme clash of clans mais rajoute un peu de l'originalité
    }

    private IEnumerator DelayLevel(BuildingInstance building, int level = 0)
    {
        yield return 0;
        building.SetLevel(level);
    }

    private void Update()
    {

        // Restart game with Shift+R
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    private void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleCombatMode()
    {
        if (SceneManager.GetActiveScene().name == "01_Game")
        {
            GameManager.Instance?.PlayerData?.Save();
            SceneManager.LoadScene("03_Combat");
        }
        else if (SceneManager.GetActiveScene().name == "03_Combat")
        {
            PlayerData?.Save();
            SceneManager.LoadScene("01_Game");
        }
    }

    [System.Serializable]
    public class MapData
    {
        public PlayerData enemyData;
        public string mapName;
        public List<BatData> batDatas;
    }

    [System.Serializable]
    public class BatData
    {
        public int x;
        public int z;
        public string id;
        public int level;
    }
}