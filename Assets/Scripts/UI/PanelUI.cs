using UnityEngine;
using UnityEngine.EventSystems;

public class PanelUI : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public string UIName => uiName;

    [SerializeField] private string uiName = "New UI";
    [SerializeField] private bool openByDefault = false;
    [SerializeField] private bool draggableWindow = false;

    private RectTransform rect;
    private Canvas canvas;
    private UserInterface ui;

    public virtual void Awake()
    {
        ui = UserInterface.Instance;
        canvas = transform.root.GetComponent<Canvas>();
        rect = GetComponent<RectTransform>();

        Register();

        if (!openByDefault)
            Hide();
    }

    public virtual void Start() { }

    public virtual void Register()
    {
        ui.RegisterUI(this);
    }

    public virtual void Unregister()
    {
        if (ui != null)
            ui.UnregisterUI(this);
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public virtual void Toogle()
    {
        if (gameObject.activeSelf) Hide();
        else Show();
    }

    public void OnDrag(PointerEventData data)
    {
        if (draggableWindow)
            SetDraggedPosition(data);
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        rect.anchoredPosition += data.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        rect.SetAsLastSibling();
    }

    private void OnDestroy()
    {
        Unregister();
    }
}
