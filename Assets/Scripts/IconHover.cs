using UnityEngine;
using UnityEngine.EventSystems;

public class IconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform icon;

    private float currentSize;
    public float setSize;

    private void Awake()
    {
        icon = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        currentSize = icon.localScale.x;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        icon.localScale = new Vector3(setSize, setSize, currentSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        icon.localScale = new Vector3(currentSize, currentSize, currentSize);
    }
}
