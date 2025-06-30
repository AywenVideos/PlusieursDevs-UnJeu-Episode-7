using System;
using DG.Tweening;
using Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public event Action<BuildingSO> OnBuildingUnlocked;
    public static BuildManager Instance { get; private set; }

    [SerializeField] private TileInstance currentBuildingInstance;
    [SerializeField] private UnitInstance currentUnitInstance;

    [Header("Tile Detection")]
    [SerializeField] private LayerMask tileLayerMask;

    [Header("Tile Instance Detection")]
    [SerializeField] private LayerMask tileInstanceLayerMask;

    public int HoveredTileX { get; private set; } = -1;
    public int HoveredTileY { get; private set; } = -1;

    private float tileSize;
    private Tween pulseTween;

    private void Awake()
    {
        Instance = this;
        tileSize = TileManager.Instance.TileSize;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if(currentBuildingInstance == null && currentUnitInstance == null)
            {
                TrySelectTileInstance();
            }else if (currentBuildingInstance != null)
            {
                PlaceBuilding();
            } else if (currentUnitInstance != null)
            {
                PlaceUnit();
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuilding();
            CancelUnitPlacing();
        }

        UpdateHoveredTile();
        UpdateHoldingTile();
        UpdateHoldingUnit();
    }

    private void TrySelectTileInstance()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, tileInstanceLayerMask))
        {
            TileInstance tile = hitInfo.collider.GetComponentInParent<TileInstance>();
            if(tile != null)
                TileManager.Instance.CurrentSelectedTile = tile;
        }else
        {
            TileManager.Instance.CurrentSelectedTile = null;
        }
    }

    public void CancelBuilding()
    {
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            pulseTween = null;
        }

        if (currentBuildingInstance != null)
            Destroy(currentBuildingInstance.gameObject);
    }

    public void CancelUnitPlacing()
    {
        if (pulseTween != null && pulseTween.IsActive())
        {
            pulseTween.Kill();
            pulseTween = null;
        }

        if (currentUnitInstance != null)
            Destroy(currentUnitInstance.gameObject);
    }

    public void UpdateHoldingTile()
    {
        if (currentBuildingInstance == null)
            return;

        currentBuildingInstance.transform.position = new Vector3(HoveredTileX * tileSize, 0.4f, HoveredTileY * tileSize);

        if (pulseTween == null || !pulseTween.IsActive())
        {
            currentBuildingInstance.transform.localScale = Vector3.one;

            pulseTween = currentBuildingInstance.transform
                .DOScale(1.05f, 0.95f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    public void UpdateHoldingUnit()
    {
        if (currentUnitInstance == null)
            return;

        currentUnitInstance.transform.position = new Vector3(HoveredTileX * tileSize, 0f, HoveredTileY * tileSize);
        
        if (pulseTween == null || !pulseTween.IsActive())
        {
            currentUnitInstance.transform.localScale = Vector3.one;

            pulseTween = currentUnitInstance.transform
                .DOScale(1.05f, 0.95f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    public void HoldUnit(UnitSO unit)
    {
        CancelUnitPlacing();
        currentUnitInstance = Instantiate(unit.unitPrefab);
    }
    
    public void HoldTile(TileSO buildingSO)
    {
        CancelBuilding();
        currentBuildingInstance = Instantiate(buildingSO.tilePrefab);

        if(currentBuildingInstance is BuildingInstance)
            (currentBuildingInstance as BuildingInstance).SetBuildMode(true);
    }
    
    /// <summary>
    /// Cette fonction sert à détruire le building actuellement selectionné seulement.
    /// </summary>
    public void DestroyBuilding()
    {
        TileInstance instance = TileManager.Instance.CurrentSelectedTile;
        int h = instance.TileSO.height;
        int w = instance.TileSO.width;

        for (int i = instance.X; i < instance.X + w; i++)
        {
            for (int j = instance.Y; j < instance.Y + h; j++)
            {
                TileManager.Instance.RemoveTile(i, j);
            }
        }

        //Check si cela correspond à une quête
        QuestManager.Instance.OnBuildDestroyed(instance);
        Toast.Notify($"Build détruit : {instance.TileSO.tileName}", Toast.Type.Normal, null, "Le build a bien été détruit", 3);
    }

    public void UnlockBuilding(UnlockNode unlockNode, BuildingSO buildingSO)
    {
        if(GameManager.Instance.PlayerData.gold < unlockNode.goldCost)
        {
            Toast.Error("Pas assez d'or !", "Vous n'avez pas assez d'or pour débloquer ce bâtiment.");
            return;
        }
        
        if(GameManager.Instance.PlayerData.milk < unlockNode.milkCost)
        {
            Toast.Error("Pas assez de lait !", "Vous n'avez pas assez de lait pour débloquer ce bâtiment.");
            return;
        }
        
        if(GameManager.Instance.PlayerData.emerald < unlockNode.emeraldCost)
        {
            Toast.Error("Pas assez d'émeraudes !", "Vous n'avez pas assez d'émeraudes pour débloquer ce bâtiment.");
            return;
        }
        
        Toast.Message("Bâtiment débloqué", $"Vous avez débloqué le bâtiment {buildingSO.tileName}.");
        buildingSO.Unlocked = true;
        OnBuildingUnlocked?.Invoke(buildingSO);
    }

    public void PlaceUnit()
    {
        if (currentUnitInstance == null)
            return;
        
        if (HoveredTileX < 0 || HoveredTileY < 0)
            return;
        
        
        if(!TileManager.Instance.CheckForSpace(HoveredTileX, HoveredTileY, 1, 1))
        {
            Toast.Error("Pas la place !", "Impossible de placer le bâtiment ici.");
            return;
        }
        
        UnitSO so = currentUnitInstance.UnitSO;
        
        CancelUnitPlacing();

        if (CombatGameManager.Instance is null)
        {
            Toast.Error("NRE", "Instance = null");
            return;
        }

        if (CombatGameManager.PlayerData is null)
        {
            Toast.Error("NRE", "PlayerData = null");
            return;
        }
        
        if (so is null)
        {
            Toast.Error("NRE", "UnitSO = null");
            return;
        }
        
        if(CombatGameManager.PlayerData.gold < so.cost)
        {
            Toast.Error("Pas assez d'or !", "Vous n'avez pas assez d'or pour construire ce bâtiment.");
            return;
        }

        
        Unit instance = TileManager.Instance.TryPlaceUnit(HoveredTileX, HoveredTileY, so);
        
        if (instance != null)
        {
            CombatGameManager.PlayerData.gold -= so.cost;
            
            instance.transform.localScale = Vector3.one * 0.7f;
            instance.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
            currentUnitInstance = null;
            AudioManager.Instance.PlayBuildClip();
        }
    }
    
    public void PlaceBuilding()
    {
        if (currentBuildingInstance == null)
            return;

        if (HoveredTileX < 0 || HoveredTileY < 0)
            return;
        
        if(!TileManager.Instance.CheckForSpace(HoveredTileX, HoveredTileY, currentBuildingInstance.TileSO.width, currentBuildingInstance.TileSO.height))
        {
            Toast.Error("Pas la place !", "Impossible de placer le bâtiment ici.");
            return;
        }

        TileSO so = currentBuildingInstance.TileSO;

        CancelBuilding();        
        
        
        TileInstance instance = TileManager.Instance.TryPlaceBuilding(HoveredTileX, HoveredTileY, so);
        
        if (instance != null)
        {
            GameManager.Instance.PlayerData.gold -= so.goldCost;
            GameManager.Instance.PlayerData.milk -= so.milkCost;
            GameManager.Instance.PlayerData.emerald -= so.emeraldCost;
            
            instance.transform.localScale = Vector3.one * 0.7f;
            instance.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBounce);
            currentBuildingInstance = null;
            AudioManager.Instance.PlayBuildClip();

            // Notify QuestManager that a building was placed
            if (instance is BuildingInstance buildingInstance && QuestManager.Instance != null)
            {
                QuestManager.Instance.OnBuildingPlaced(buildingInstance);
            }

            // Refresh the shop UI after a building is placed
            if (ShopPanelUI.Instance != null)
            {
                ShopPanelUI.Instance.ShowTilesInShop();
            }
        }
    }

    private void UpdateHoveredTile()
    {
        HoveredTileX = -1;
        HoveredTileY = -1;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, tileLayerMask))
        {
            TileInstance tile = hitInfo.collider.GetComponentInParent<TileInstance>();
            if (tile != null)
            {
                HoveredTileX = tile.X;
                HoveredTileY = tile.Y;
            }
        }
    }

    public bool IsHoldingBuilding()
    {
        return currentBuildingInstance != null;
    }
}