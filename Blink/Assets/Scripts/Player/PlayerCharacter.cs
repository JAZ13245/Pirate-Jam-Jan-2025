using KinematicCharacterController;
using UnityEngine;

public enum Stance
{
    Stand, Crouch
}
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool Crouch;
    public bool CrouchHeld;
    public bool crouchToggleable;
}
public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 20f;
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;
    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [Range(0, 1f)] [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0, 1f)] [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    [Header("Components")]
    [SerializeField] private Transform root;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;

    private Stance _stance;
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedCrouch;

    // Sets Up Player Movement
    public void Intialize()
    {
        _stance = Stance.Stand;

        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        // Take the 2D input vector and create a 3D movement vector on the XZ plane
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        // Clamp the Length to 1 to prevent movement faster diagonally with WASD input
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        // Orient the input so it's relative to the direction the player is facing.
        _requestedMovement = input.Rotation * _requestedMovement;

        _requestedJump = _requestedJump || input.Jump;

        if(input.Crouch && input.crouchToggleable) _requestedCrouch = !_requestedCrouch; // Toggle Crouch
        else if (!input.crouchToggleable) _requestedCrouch = input.CrouchHeld; // NonToggle Crouch
    }

    public void UpdateBody()
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        // Get Camera Target Height by Stance State
        var cameraTargetHeight = currentHeight * 
        (
            _stance is Stance.Stand
                ? standCameraTargetHeight
                : crouchCameraTargetHeight
        );
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = new Vector3(0f, cameraTargetHeight, 0f);
        root.localScale = rootTargetScale;
    }

    // Rotates the Physical Character
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // Update the character's rotation to face in the same direction as the
        // requested rotation (which would be the camera rotation)

        // We don't want the character to pitch up and down, so the direction the character looks should be "flattened."

        // This is done by projecting a vector pointing in the same direction that the player is looking onto a flat ground plane
        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if(forward != Vector3.zero) currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    // Movement
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        // If on the ground...
        if(motor.GroundingStatus.IsStableOnGround)
        {
            // Snap the requested movement direction to the angle of the surface the character is currently walking on
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            var speed = _stance is Stance.Stand
                ? walkSpeed
                : crouchSpeed;

            // And move along the ground in that direction.
            currentVelocity = groundedMovement * speed;
        }else // If in the air
        {
            // Gravity
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if(_requestedJump)
        {
            _requestedJump = false;

            // Unstick the player from the ground.
            motor.ForceUnground(time: 0f);

            // Set Minimum Vertical Speed to the Jump Speed
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            // Add the difference in current and target vertical speed to the character's velocity
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        // Crouch
        if(_requestedCrouch && _stance is Stance.Stand)
        {
            // Shrink Character
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: 0
            );
            
            _stance = Stance.Crouch;
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch
        if(!_requestedCrouch && _stance is not Stance.Stand)
        {
            // Shrink Character
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: 0
            );
            _stance = Stance.Stand;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {}

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {}

    public void PostGroundingUpdate(float deltaTime)
    {}

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {}

    // Used for PlayerCamera Script Intialization
    public Transform GetCameraTarget() => cameraTarget;
}
