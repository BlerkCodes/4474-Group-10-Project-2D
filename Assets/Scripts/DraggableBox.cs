using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor;

public class DraggableBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public TextMeshProUGUI text;
    private BoxValue bv;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform baseParent;

    private void Awake()
    {
        baseParent = transform.parent;
        bv = gameObject.GetComponent<BoxValue>();
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
        if (eventData.pointerEnter == null || !eventData.pointerEnter.TryGetComponent<BoxSlot>(out BoxSlot bs))
        {
            transform.SetParent(baseParent);
            text.fontSize = 20;
        }
        else
        {
            if (parentAfterDrag.transform == baseParent.transform)
            {
                transform.SetParent(parentAfterDrag);
                text.fontSize = 20;
            }
            else if (parentAfterDrag.transform.position.y == baseParent.transform.position.y) // I know this seems stupid but it works
            {
                parentAfterDrag = baseParent;
                transform.SetParent(parentAfterDrag);
                text.fontSize = 20;
            }
            else
            {
                transform.SetParent(parentAfterDrag);
                text.fontSize = 8;
                bv.isDropped();
            }
        }

        image.raycastTarget = true;
        text.raycastTarget = true;
    }
}
