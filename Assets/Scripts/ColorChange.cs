using UnityEngine;

public class ColorChange : MonoBehaviour
{
    private SpriteRenderer sp;

    private void Awake()
    {
        sp = gameObject.GetComponent<SpriteRenderer>();
        sp.color = Color.orange;
    }

    public void ChangeComplete()
    {
        sp.color = Color.green;
    }

    public void ChangeCurrent()
    {
        sp.color = Color.yellow;
    }

    public void ChangeSkipped()
    {
        sp.color = Color.gray;
    }
}
