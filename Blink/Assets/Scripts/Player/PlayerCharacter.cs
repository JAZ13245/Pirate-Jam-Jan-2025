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
    public bool JumpHeld;
    public bool Crouch;
    public bool CrouchHeld;
    public bool crouchToggleable;
}
public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float walkResponse = 25f;
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 20f;
    [Range(0, 1f)] [SerializeField] private float jumpHeldGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Header("In Air Settings")]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float crouchResponse = 20f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
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
    private bool _requestedJumpHeld;
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

        // Jumping
        _requestedJump = _requestedJump || input.Jump;
        _requestedJumpHeld = input.JumpHeld;

        if(input.Crouch && input.crouchToggleable) _requestedCrouch = !_requestedCrouch; // Toggle Crouch
        else if (!input.crouchToggleable) _requestedCrouch = input.CrouchHeld; // NonToggle Crouch
    }

    public void UpdateBody(float deltaTime)
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

        cameraTarget.localPosition = Vector3.Lerp
        (
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );
        root.localScale = Vector3.Lerp
        (
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );
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

            // Calculate the speed and responsivness based on stance
            var speed = _stance is Stance.Stand
                ? walkSpeed
                : crouchSpeed;
            var response = _stance is Stance.Stand
                ? walkResponse
                : crouchResponse;
            

            // And move along the ground in that direction.
            var targetVelocity = groundedMovement * speed;
            currentVelocity = Vector3.Lerp
            (
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-response * deltaTime)
            );
        }else // If in the air
        {
            // In Air Movement
            if(_requestedMovement.sqrMagnitude > 0f)
            {
                // Requested Movement Projected onto Movement Plane
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.magnitude;

                // Current Velocity on Movement Plane
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                // Calculate Movement Force
                var movementForce = planarMovement * airAcceleration * deltaTime;

                // Add it to the current planar velocity for a target velocity
                var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                // Limit Target Velocity to Air Speed
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                // Steer Towards Current Velocity
                currentVelocity += targetPlanarVelocity - currentPlanarVelocity;
            }
            // Gravity
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if(_requestedJumpHeld && verticalSpeed > 0f) // Allows for user to hold jump to jump higher
            {
                effectiveGravity *= jumpHeldGravity;
            }
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
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
    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
        {
            motor.BaseVelocity = Vector3.zero;
        }
    }
}
