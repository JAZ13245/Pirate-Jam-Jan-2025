using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Header("Camera Spring Settings")]
    [Min(0.01f)] [SerializeField] private float halfLife = 0.075f;
    [SerializeField] private float frequency = 18f;
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    private Vector2 _springPosition;
    private Vector2 _springVelocity;

    public void Intialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }
    
    public void UpdateSpring(float deltaTime)
    {
        Spring(ref _springPosition, ref _springVelocity, transform.position, halfLife, frequency, deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, _springPosition);
        Gizmos.DrawSphere(_springPosition, 0.01f);
    }
    

    // Source: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
    public void Spring(ref Vector2 current, ref Vector2 velocity, Vector2 target, float dampingRatio, float angularFrequency, float timeStep)
    {
        dampingRatio = -Mathf.Log(0.5f) / (frequency * halfLife);
        float f = 1.0f + 2.0f * timeStep * dampingRatio * angularFrequency;
        float oo = angularFrequency * angularFrequency;
        float hoo = timeStep * oo;
        float hhoo = timeStep * hoo;
        float detInv = 1.0f / (f + hhoo);
        var detX = f * current + timeStep * velocity + hhoo * target;
        var detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}
