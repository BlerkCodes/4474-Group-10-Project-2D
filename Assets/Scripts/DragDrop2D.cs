using UnityEngine;
using UnityEngine.InputSystem;

public class DragDrop2D : MonoBehaviour
{
    private Camera cam;
    private bool isDragging;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandleMouseDown();
        HandleDragging();
        HandleMouseUp();
    }

    void HandleMouseDown()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (GetMouseWorldPosition(out Vector3 worldPos))
        {
            if (IsMouseOverThisObject(worldPos))
            {
                isDragging = true;
                offset = transform.position - worldPos;
            }
        }
    }

    void HandleDragging()
    {
        if (!isDragging || !Mouse.current.leftButton.isPressed) return;

        if (GetMouseWorldPosition(out Vector3 worldPos))
        {
            transform.position = worldPos + offset;
        }
    }

    void HandleMouseUp()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    bool GetMouseWorldPosition(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        if (cam == null) return false;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        worldPos = cam.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Mathf.Abs(cam.transform.position.z))
        );

        return true;
    }

    bool IsMouseOverThisObject(Vector3 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        Debug.Log(hit);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
}