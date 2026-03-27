using TMPro;
using UnityEngine;

public class BoxValue : MonoBehaviour
{
    private TextMeshProUGUI text;
    public int currentValue = 0;

    private void Awake()
    {
        text = this.GetComponentInChildren<TextMeshProUGUI>();
        text.SetText(currentValue.ToString());
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
}
