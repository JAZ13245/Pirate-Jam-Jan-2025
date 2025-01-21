using UnityEngine;

public struct CameraInput
{
    public Vector2 Look;
}
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensitvity = 0.1f;

    private Vector3 _eulerAngels;

    // Sets Up Player Camera
    public void Intialize(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;

        transform.eulerAngles = _eulerAngels = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngels += new Vector3(-input.Look.y, input.Look.x) * sensitvity;
        transform.eulerAngles = _eulerAngels;
    }
    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
