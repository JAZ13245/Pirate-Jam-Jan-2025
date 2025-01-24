using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Header("Camera Spring Settings")]
    [Min(0.01f)] [SerializeField] private float halfLife = 0.075f;
    [SerializeField] private float frequency = 18f;
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    private Vector3 _springPosition;
    private Vector3 _springVelocity;

    public void Initialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }
    
    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        Spring(ref _springPosition, ref _springVelocity, transform.position, halfLife, frequency, deltaTime);

        var localSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up);

        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
    }
    

    // Source: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
    public void Spring(ref Vector3 current, ref Vector3 velocity, Vector2 target, float dampingRatio, float angularFrequency, float timeStep)
    {
        dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        float f = 1.0f + 2.0f * timeStep * dampingRatio * angularFrequency;
        float oo = angularFrequency * angularFrequency;
        float hoo = timeStep * oo;
        float hhoo = timeStep * hoo;
        float detInv = 1.0f / (f + hhoo);

        // Convert Vector2 target to Vector3
        Vector3 target3D = new Vector3(target.x, target.y, 0);

        // Calculate detX and detV
        var detX = f * current + timeStep * velocity + hhoo * target3D;
        var detV = velocity + hoo * (target3D - current);

        // Update current and velocity
        current = detX * detInv;
        velocity = detV * detInv;
    }
}
