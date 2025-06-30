using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCamera : MonoBehaviour
{
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    public static Camera cam;
    private Plane dragPlane;
    private Vector3 dragStartPoint;
    private bool isDragging = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;

        // Vue isométrique typique
        transform.rotation = Quaternion.Euler(30, 45, 0);
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    void HandleDrag()
    {
        // Plan horizontal à la hauteur 0
        dragPlane = new Plane(Vector3.up, Vector3.zero);

        // Début du drag
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                dragStartPoint = ray.GetPoint(enter);
                isDragging = true;
            }
        }

        // Drag en cours
        if (Input.GetMouseButton(0) && isDragging)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 currentPoint = ray.GetPoint(enter);
                Vector3 delta = dragStartPoint - currentPoint;
                Vector3 newPosition = transform.position + delta;
                int clampRadius = 45;
                newPosition.y = transform.position.y;
                newPosition.z = Mathf.Clamp(newPosition.z, -clampRadius, clampRadius);
                newPosition.x = Mathf.Clamp(newPosition.x, -clampRadius, clampRadius);

                transform.position = newPosition;
            }
        }

        // Fin du drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}
