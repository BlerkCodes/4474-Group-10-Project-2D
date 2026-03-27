using UnityEngine;

public class ScaleAdjuster : MonoBehaviour
{
    private float handleValue;
    private Vector3 leftValue;
    private Vector3 rightValue;

    public RectTransform ScaleHandle;
    public RectTransform leftScale;
    public RectTransform rightScale;

    public float speed = 1;
    private Vector3 scaleVelocity = Vector3.zero;
    private float handleVelocity = 0f;


    // Update is called once per frame
    void Update()
    {
        float angle = Mathf.SmoothDampAngle(ScaleHandle.transform.localEulerAngles.z, handleValue, ref handleVelocity, speed * Time.deltaTime);
        ScaleHandle.transform.localRotation = Quaternion.Euler(0, 0, angle);
        leftScale.transform.localPosition = Vector3.SmoothDamp(leftScale.transform.localPosition, leftValue, ref scaleVelocity, speed * Time.deltaTime);
        rightScale.transform.localPosition = Vector3.SmoothDamp(rightScale.transform.localPosition, rightValue, ref scaleVelocity, speed * Time.deltaTime);
    }

    public void ChangeCenter()
    {
        handleValue = 0f;
        leftValue = new Vector3(-200, 30, 0);
        rightValue = new Vector3(200, 30, 0);
    }

    public void ChangeRight()
    {
        handleValue = -10f;
        leftValue = new Vector3(-200, 60, 0);
        rightValue = new Vector3(200, 0, 0);
    }

    public void ChangeLeft()
    {
        handleValue = 10f;
        leftValue = new Vector3(-200, 0, 0);
        rightValue = new Vector3(200, 60, 0);
    }
}
