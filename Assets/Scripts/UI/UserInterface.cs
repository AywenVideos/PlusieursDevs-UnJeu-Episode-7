using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using System.Security.Cryptography;
using Entities;

[DefaultExecutionOrder(-10)]
public class UserInterface : MonoBehaviour
{
    #region Properties

    public static UserInterface Instance { get; private set; }

    public ResourceItemUI GoldResource => goldResource;
    public ResourceItemUI EmeraldResource => emeraldResource;
    public ResourceItemUI MilkResource => milkResource;
    public ResourceItemUI NPCResource => npcResource;

    #endregion

    #region Variables

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI version;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private ToastUIManager toastManager;
    [SerializeField] private Sprite emeraldIconSprite; // New field for emerald icon
    [SerializeField] private GameObject destroyButton;
    [SerializeField] private QuestPanelUI _questPanelUi;
    [SerializeField] private UnitPanelUI _unitPanelUi;

    [Header("Resources References")]
    [SerializeField] private ResourceItemUI goldResource;
    [SerializeField] private ResourceItemUI emeraldResource;
    [SerializeField] private ResourceItemUI milkResource;
    [SerializeField] private ResourceItemUI npcResource;
    [SerializeField] private BuildingSO laboratoryBuildingSO;

    [Header("Selection System")]
    [SerializeField] private TextMeshProUGUI selectedTileText;
    [SerializeField] private Color selectionBoxColor = new(0f, 1f, 0f, 0.8f);
    [SerializeField] private float lineWidth = 3.5f;
    [SerializeField] private float boundsExpansion = 0.2f;
    [SerializeField] private float minDistanceForUpdate = 0.5f; // Distance minimale d'update lors du zoom
    [SerializeField] private float minLinePixelWidth = 1.5f; // Largeur minimale en pixels

    #endregion

    private string currentHoverText;
    private List<PanelUI> PanelUIs = new();
    private Camera worldCamera;
    private TileInstance selectedTileInstance;
    private NPC selectedNPC;
    private Unit selectedUnit;
    private GameObject currentSelectedObject;
    private Material lineMaterial;
    private Bounds selectionBounds;
    private bool hasValidSelection;
    private GameObject selectionBoxObject;
    private LineRenderer[] boxLines;
    private Vector3 lastObjectPosition;
    private float lastUpdateTime;
    private Bounds lastBounds;
    private Vector3 lastCameraPosition;
    private TileSO grassTileSO; // Cache for the grass tile

    private void Awake()
    {
        Instance = this;

        worldCamera = Camera.main;

        if (worldCamera != null)
            lastCameraPosition = worldCamera.transform.position;

        version.text = $"COA — {Application.version}";

        if (Application.isBatchMode)
        {
            gameObject.SetActive(false);
        }
        
        // Create selection line material
        CreateLineMaterial();

        // Load the grass tile SO
        grassTileSO = TileSODatabase.GetBySlug("grass_01");
        if (grassTileSO == null)
        {
            Debug.LogError("Failed to load grass tile SO from TileDatabase");
        }
    }

    private void Start()
    {
        if (_unitPanelUi) _unitPanelUi.Show();
    }

    private void CreateLineMaterial()
    {
        // Create a simple unlit material for drawing lines
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = selectionBoxColor;
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        
        // Alternative: Use the built-in shader if available
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null)
        {
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
        
        CreateSelectionBox();
    }
    
    private void CreateSelectionBox()
    {
        // Create parent object for selection box
        selectionBoxObject = new GameObject("SelectionBox");
        selectionBoxObject.SetActive(false);
        
        // Create 12 LineRenderers for the 12 edges of a box
        boxLines = new LineRenderer[12];
        
        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject($"BoxLine_{i}");
            lineObj.transform.SetParent(selectionBoxObject.transform);
            
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = selectionBoxColor;
            lr.endColor = selectionBoxColor;
            lr.startWidth = lineWidth * 0.015f; // Augmenté pour des lignes plus visibles
            lr.endWidth = lineWidth * 0.015f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            
            boxLines[i] = lr;
        }
    }

    private void UpdateSelectedTileTextPosition()
    {
        if (selectedTileInstance != null)
        {
            Vector3 worldPos = selectedTileInstance.transform.position + selectedTileInstance.TileSO.offsetSelectedTileText;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
            selectedTileText.rectTransform.position = screenPoint;
        }
        else if (selectedNPC != null)
        {
            // Position du texte au-dessus du NPC - augmenté à 2.5 unités
            Vector3 worldPos = selectedNPC.transform.position + Vector3.up * 2.5f;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPos);
            selectedTileText.rectTransform.position = screenPoint;
        }
    }
    
    private void CalculateSelectionBounds()
    {
        if (currentSelectedObject == null)
        {
            hasValidSelection = false;
            return;
        }
        
        // Get all renderers in the object and its children
        Renderer[] renderers = currentSelectedObject.GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            // If no renderers, use colliders as fallback
            Collider[] colliders = currentSelectedObject.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                selectionBounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    selectionBounds.Encapsulate(colliders[i].bounds);
                }
                hasValidSelection = true;
            }
            else
            {
                // Last resort: use transform position with a small default size
                selectionBounds = new Bounds(currentSelectedObject.transform.position, Vector3.one);
                hasValidSelection = true;
            }
        }
        else
        {
            // Initialize with first renderer's bounds
            selectionBounds = renderers[0].bounds;
            
            // Combine with all other renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                selectionBounds.Encapsulate(renderers[i].bounds);
            }
            hasValidSelection = true;
        }
        
        // Expand bounds for visual clarity
        if (hasValidSelection)
        {
            selectionBounds.Expand(boundsExpansion);
        }
    }
    
    private bool ShouldUpdateSelectionBox()
    {
        // Check if the object has moved significantly or camera has zoomed/panned
        if (currentSelectedObject != null && lastObjectPosition != currentSelectedObject.transform.position)
            return true;

        if (worldCamera != null && Vector3.Distance(lastCameraPosition, worldCamera.transform.position) > minDistanceForUpdate)
            return true;

        return false;
    }

    private void UpdateSelectionBox()
    {
        if (!hasValidSelection || selectionBoxObject == null)
        {
            if (selectionBoxObject != null)
                selectionBoxObject.SetActive(false);
            return;
        }

        selectionBoxObject.SetActive(true);

        // Calculate world corners of the bounds
        Vector3 min = selectionBounds.min;
        Vector3 max = selectionBounds.max;

        // Define corners of the box
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(min.x, min.y, min.z); // Front Bottom Left
        corners[1] = new Vector3(max.x, min.y, min.z); // Front Bottom Right
        corners[2] = new Vector3(max.x, max.y, min.z); // Front Top Right
        corners[3] = new Vector3(min.x, max.y, min.z); // Front Top Left
        corners[4] = new Vector3(min.x, min.y, max.z); // Back Bottom Left
        corners[5] = new Vector3(max.x, min.y, max.z); // Back Bottom Right
        corners[6] = new Vector3(max.x, max.y, max.z); // Back Top Right
        corners[7] = new Vector3(min.x, max.y, max.z); // Back Top Left

        // Draw 12 lines for the box edges
        // Front face
        DrawLine(boxLines[0], corners[0], corners[1]);
        DrawLine(boxLines[1], corners[1], corners[2]);
        DrawLine(boxLines[2], corners[2], corners[3]);
        DrawLine(boxLines[3], corners[3], corners[0]);

        // Back face
        DrawLine(boxLines[4], corners[4], corners[5]);
        DrawLine(boxLines[5], corners[5], corners[6]);
        DrawLine(boxLines[6], corners[6], corners[7]);
        DrawLine(boxLines[7], corners[7], corners[4]);

        // Connecting edges
        DrawLine(boxLines[8], corners[0], corners[4]);
        DrawLine(boxLines[9], corners[1] , corners[5]);
        DrawLine(boxLines[10], corners[2], corners[6]);
        DrawLine(boxLines[11], corners[3], corners[7]);

        // Update line widths based on camera distance
        float distance = Vector3.Distance(worldCamera.transform.position, selectionBounds.center);
        float perceivedWidth = Mathf.Max(lineWidth / distance, minLinePixelWidth / worldCamera.pixelHeight); // Adjust for pixel perfect
        foreach (LineRenderer lr in boxLines)
        {
            lr.startWidth = perceivedWidth;
            lr.endWidth = perceivedWidth;
        }

        lastObjectPosition = currentSelectedObject.transform.position;
        lastCameraPosition = worldCamera.transform.position;
    }

    private void DrawLine(LineRenderer lr, Vector3 start, Vector3 end)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    public void SelectObject(GameObject obj, string displayText)
    {
        currentSelectedObject = obj;
        selectedTileText.text = displayText;
        selectedTileText.gameObject.SetActive(true);
        CalculateSelectionBounds();
        UpdateSelectedTileTextPosition();
        UpdateSelectionBox();
    }

    private void ClearSelection()
    {
        currentSelectedObject = null;
        hasValidSelection = false;
        selectedTileInstance = null;
        selectedNPC = null;
        
        if (selectionBoxObject != null)
            selectionBoxObject.SetActive(false);
        
        if (selectedTileText != null)
        {
            selectedTileText.text = "";
            selectedTileText.gameObject.SetActive(false);
        }
    }

    public void SetSelectedTileInstance(TileInstance tileInstance)
    {
        selectedTileInstance = tileInstance;
        selectedNPC = null;
        
        ClearSelection();
        if (StatsPanelUI.Instance != null)
            StatsPanelUI.Instance.Hide();
        if (LaboratoryPanelUI.Instance != null)
            LaboratoryPanelUI.Instance.Hide();

        if (tileInstance != null)
        {
            SelectObject(tileInstance.gameObject, tileInstance.TileSO.tileName);
        }
        UpdateSelectedTileTextPosition();

        if (tileInstance is BuildingInstance building)
        {
            if (building.BuildingSO == laboratoryBuildingSO)
            {
                if (LaboratoryPanelUI.Instance != null)
                {
                    LaboratoryPanelUI.Instance.Show();
                }
            }
            else
            {
                if (StatsPanelUI.Instance != null)
                {
                    StatsPanelUI.Instance.ShowForBuilding(building);
                }
            }
        }
    }
    
    public void SetSelectedNPC(NPC npc)
    {
        selectedTileInstance = null;
        selectedNPC = npc;
        
        if (npc == null)
        {
            ClearSelection();
            if (StatsPanelUI.Instance != null)
                StatsPanelUI.Instance.Hide();
            return;
        }
        
        SelectObject(npc.gameObject, npc.GetDisplayName());
        UpdateSelectedTileTextPosition();
        
        if (StatsPanelUI.Instance != null)
            StatsPanelUI.Instance.Hide();
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedTileInstance = null;
        selectedUnit = unit;
        
        if (unit == null)
        {
            ClearSelection();
            if (StatsPanelUI.Instance != null)
                StatsPanelUI.Instance.Hide();
            return;
        }
        
        SelectObject(unit.gameObject, unit.GetDisplayName());
        UpdateSelectedTileTextPosition();
        
        if (StatsPanelUI.Instance != null)
            StatsPanelUI.Instance.Hide();
    }

    public void FadeToBlack(UnityAction onComplete = null)
    {
        StartCoroutine(Fade(1, onComplete));
    }

    public void FadeFromBlack(UnityAction onComplete = null)
    {
        StartCoroutine(Fade(0, onComplete));
    }

    private IEnumerator Fade(float targetAlpha, UnityAction onComplete)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // Appeler la callback après le fade
        onComplete?.Invoke();
    }

    public void ShowHoverText(string text)
    {
        currentHoverText = text;
    }

    public void RegisterUI(PanelUI panel)
    {
        if (PanelUIs.Contains(panel))
        {
            Debug.LogError($"{panel.UIName} is already registered in PanelUIs", panel);
            return;
        }

        PanelUIs.Add(panel);
    }

    public void UnregisterUI(PanelUI panel)
    {
        if (!PanelUIs.Contains(panel))
        {
            Debug.LogError($"{panel.UIName} is not registered in PanelUIs", panel);
            return;
        }

        PanelUIs.Remove(panel);
    }

    public T GetUI<T>(string uiName) where T : PanelUI
    {
        return PanelUIs.Where(p => p.UIName == uiName).FirstOrDefault() as T;
    }

    /// <summary>
    /// Toggle on or of the Destroy Building Button
    /// </summary>
    /// <param name="Value"></param>
    public void ToggleDestroyButton(bool Value)
    {
        if (destroyButton) destroyButton.SetActive(Value);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Toast.Warning("Expulsion !", "Vous avez été expulsé du serveur.");
        }

        if(Input.GetKeyDown(KeyCode.I) && _questPanelUi != null)
        {
            _questPanelUi.Toogle();
        }

        UpdateSelectedTileTextPosition();
        
        // Update selection box seulement si nécessaire (pour éviter le flickering)
        if (ShouldUpdateSelectionBox())
        {
            CalculateSelectionBounds();
            UpdateSelectionBox();
        }
    }
    
    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            DestroyImmediate(lineMaterial);
        }
        
        if (selectionBoxObject != null)
        {
            DestroyImmediate(selectionBoxObject);
        }
    }
}
