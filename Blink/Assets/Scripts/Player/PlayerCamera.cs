using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitvity = 0.1f;

    private float InputX;
    private float InputY;
    [SerializeField] private float lowerClamp = -90f;
    [SerializeField] private float upperClamp = 90f;

    // Sets Up Player Camera
    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;

        //transform.eulerAngles = _eulerAngels = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        InputX -= input.Look.y * sensitvity;
        InputY += input.Look.x * sensitvity;

        InputX = Mathf.Clamp(InputX, lowerClamp, upperClamp);

        transform.rotation = Quaternion.Euler(InputX, InputY, 0f);

        //_eulerAngels += new Vector3(InputY, InputX);
    }
    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
