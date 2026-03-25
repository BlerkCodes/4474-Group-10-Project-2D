using UnityEngine;
using UnityEngine.EventSystems;

public class BoxSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableBox draggableBox = dropped.GetComponent<DraggableBox>();
            draggableBox.parentAfterDrag = transform;
        }
    }
}
