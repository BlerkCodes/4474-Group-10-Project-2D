using TMPro;
using UnityEngine;

public class BoxValue : MonoBehaviour
{
    private TextMeshProUGUI text;
    public int currentValue = 0;
    public int dropped;

    private void Awake()
    {
        text = this.GetComponentInChildren<TextMeshProUGUI>();
        text.SetText(currentValue.ToString());
        dropped = 0;
    }

    public void SetValue(int value)
    {
        currentValue = value;
        text.SetText(currentValue.ToString());
    }

    public int GetValue()
    {
        return currentValue;
    }

    public void isDropped()
    {
        dropped++;
    }

    public int GetDropped()
    {
        return dropped;
    }

    public void ResetDrops()
    {
        dropped = 0;
    }
}
