using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

public class DraggableBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public TextMeshProUGUI text;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform baseParent;

    private void Awake()
    {
        baseParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        text.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null)
        {
            transform.SetParent(baseParent);
            text.fontSize = 20;
        }
        else
        {
            transform.SetParent(parentAfterDrag);
            if (eventData.pointerEnter == baseParent)
            {
                text.fontSize = 20;
            }
            else
            {
                text.fontSize = 8;
            }
        }

        image.raycastTarget = true;
        text.raycastTarget = true;
    }
}
