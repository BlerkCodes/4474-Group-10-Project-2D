using TMPro;
using UnityEngine;

public class Weight : MonoBehaviour
{
    public int length { get; private set; }
    public bool flipped { get; private set; }

    private Transform t;
    private TextMeshProUGUI tm;
    [SerializeField]
    private Transform weightLengthTransform;

    const float SIZE_PER_UNIT = 0.2f; // Width size for Transform for a pipe length of 1
    const float HEIGHT = 0.5f;
    private void Awake()
    {
        t = GetComponent<Transform>();
        tm = GetComponentInChildren<TextMeshProUGUI>();

        weightLengthTransform.localScale = new Vector2(SIZE_PER_UNIT, HEIGHT);
    }
   
    private void Start()
    {
        // Delete later
        SetNewPipeOrientation(10);
    }

    void SetNewPipeOrientation(int length)
    {
        SetLength(length);
        tm.text = length.ToString();
    }

    public void SetLength(int len)
    {
        length = len;
        weightLengthTransform.localScale = new Vector2(SIZE_PER_UNIT * len, HEIGHT);

        /*
        // Reset the positions of the pipe ends to the end of the pipe
        foreach (Transform rt in pipeLengthTransform)
        {
            float sizeX = rt.sizeDelta.x;
            float sizeY = rt.sizeDelta.y;
            int flipFactor = 1; // Whether or not the pipe end is going up (1) or down (-1)

            if (rt.name == "Down")
            {
                flipFactor = -1;
            }

            float posX = (pipeLengthTransform.sizeDelta.x / 2 + sizeX / 2) * flipFactor;
            float posY = sizeX / 2 * flipFactor;

            rt.anchoredPosition = new Vector2(posX, posY);
        }
        */
    }

    public void Flip()
    {
        flipped = !flipped;

        if (flipped)
        {
            t.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            t.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
