using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float walkStrength = 1f;
    [SerializeField] private float slideStrength = 1.75f;
    [SerializeField] private float strengthResponse = 5f;
    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVelocity;
    private float _smoothStrenth;
    public void Initialize()
    {
        _smoothStrenth = walkStrength;
    }

    public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        var playerAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        var damping = playerAcceleration.magnitude > _dampedAcceleration.magnitude
            ? attackDamping
            : decayDamping;
        
        _dampedAcceleration = Vector3.SmoothDamp
        (
            current: _dampedAcceleration,
            target: playerAcceleration,
            currentVelocity: ref _dampedAccelerationVelocity,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        // Get the rotation axis based on the acceleration vector
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;

        // Reset the rotation to that of it's parent.
        transform.localRotation = Quaternion.identity;

        // Rotate around the lean axis
        var targetStrength = sliding
            ? slideStrength
            : walkStrength;
        
        _smoothStrenth = Mathf.Lerp(_smoothStrenth, targetStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));

        transform.rotation = Quaternion.AngleAxis(-_dampedAcceleration.magnitude * _smoothStrenth, leanAxis) * transform.rotation;
    }
}
