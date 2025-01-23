using KinematicCharacterController;
using UnityEngine;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpHeld;
    public bool Crouch;
    public bool CrouchHeld;
    public bool crouchToggleable;
    public bool Blink;
    public bool BlinkHeld;
    public bool BlinkReleased;
}
public enum Stance
{
    Stand, Crouch, Slide
}
public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
}
public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float walkResponse = 25f;
    [Header("Jump Settings")]
    [SerializeField] private float jumpSpeed = 20f;
    [Range(0, 1f)] [SerializeField] private float jumpHeldGravity = 0.4f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float gravity = -90f;
    [Header("In Air Settings")]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float crouchResponse = 20f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float ceilingCheckRadius = 0.5f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Range(0, 1f)] [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0, 1f)] [SerializeField] private float crouchCameraTargetHeight = 0.7f;
    [Header("Blink Settings")]
    [SerializeField] private float blinkTimeThreshold = 0.5f;
    [SerializeField] private float maxBlinkTime = 3f;
    [SerializeField] private float baseBlinkDistance = 15f;
    [SerializeField] private float maxBlinkDistance = 30f;
    [SerializeField] private GameObject testBlinkEffect;
    [Header("Sliding Settings")]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;

    [Header("Components")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private Transform root;
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;
    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedJumpHeld;
    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;
    private bool _requestedBlink;
    private bool _requestedBlinkHeld;
    private bool _requestedBlinkRelease;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequest;
    private bool _ungroundeDueToJump;

    // Blink Timer Variables
    private float _currentBlinkTime;

    // Sets Up Player Movement
    public void Intialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;

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
        var wasRequestingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if(_requestedJump && !wasRequestingJump)
        {
            _timeSinceJumpRequest = 0f;
        }
        _requestedJumpHeld = input.JumpHeld;

        // Crouching
        var wasRequestingCrouch = _requestedCrouch;
        if(input.Crouch && input.crouchToggleable) _requestedCrouch = !_requestedCrouch; // Toggle Crouch
        else if (!input.crouchToggleable) _requestedCrouch = input.CrouchHeld; // NonToggle Crouch

        if(_requestedCrouch && !wasRequestingCrouch)
        {
            _requestedCrouchInAir = !_state.Grounded;
        }
        else if(!_requestedCrouch && wasRequestingCrouch)
        {
            _requestedCrouch = false;
        }

        // Blinking
        _requestedBlink = _requestedBlink || input.Blink;
        _requestedBlinkHeld = input.BlinkHeld;
        _requestedBlinkRelease = input.BlinkReleased;
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        // Get Camera Target Height by Stance State
        var cameraTargetHeight = currentHeight * 
        (
            _state.Stance is Stance.Stand
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
            _timeSinceUngrounded = 0f;
            _ungroundeDueToJump = false;

            // Snap the requested movement direction to the angle of the surface the character is currently walking on
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            // Slide Detection
            // Get Current State of the Player
            var moving = groundedMovement.sqrMagnitude > 0f;
            var crouching = _state.Stance is Stance.Crouch;
            var wasStanding = _lastState.Stance is Stance.Stand;
            var wasInAir = !_lastState.Grounded;
            if(moving && crouching && (wasStanding || wasInAir))
            {
                // Activate SLiding
                _state.Stance = Stance.Slide;

                // When landing on stable ground the character motor projects the velocity onto a flat ground plane
                if(wasInAir)
                {
                    currentVelocity = Vector3.ProjectOnPlane
                    (
                        vector: _lastState.Velocity,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    );
                }

                var effectiveSlideStartSpeed = slideStartSpeed;
                if(!_lastState.Grounded && !_requestedCrouchInAir)
                {
                    effectiveSlideStartSpeed = 0f;
                    _requestedCrouchInAir = false;
                }
                
                var slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);
                currentVelocity = motor.GetDirectionTangentToSurface
                (
                    direction: currentVelocity,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                ) * slideSpeed;
            }


            // Regular Movement
            if(_state.Stance is Stance.Stand or Stance.Crouch)
            {
                // Calculate the speed and responsivness based on stance
                var speed = _state.Stance is Stance.Stand
                    ? walkSpeed
                    : crouchSpeed;
                var response = _state.Stance is Stance.Stand
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
            }
            else // Continue Sliding
            {
                // Friction
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                // Slope
                var force = Vector3.ProjectOnPlane
                (
                    vector: -motor.CharacterUp,
                    planeNormal: motor.GroundingStatus.GroundNormal
                ) * slideGravity;
                currentVelocity -= force * deltaTime;

                // Steering
                // Target Velocity is the Player's movement direction, at the current speed
                var currentSpeed = currentVelocity.magnitude;
                var targetVelocity = groundedMovement * currentSpeed;
                var steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;
                // Add Steer Force, but clamp speed so the slide speed doesn't increase due to direct movement input
                currentVelocity += steerForce;
                currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);

                // Stop
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Crouch;
                }
            }
        }else // If in the air
        {
            _timeSinceUngrounded += deltaTime;

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
                // Will be changed depending on current velocity
                var movementForce = planarMovement * airAcceleration * deltaTime;

                // If moving slower than the max air speed, treat movement force as a simple steering force.
                if(currentPlanarVelocity.magnitude < airSpeed)
                {
                    // Add it to the current planar velocity for a target velocity
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    // Limit Target Velocity to Air Speed
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    // Steer Towards Target Velocity
                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                // Otherwise, nerf the movement force when it is in the direction of the current planar velocity
                else if(Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    // Project movement force onto the plane whose normal is the current planar velocity
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                // Prevent Air-Climbing Steep Slopes
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    // If moving in the same direction as the resultant velocity
                    if(Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        // Calculate obstruction normal
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            motor.GroundingStatus.GroundNormal
                        ).normalized;

                        // Project Movement force onto obstruction plane
                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }

                currentVelocity += movementForce;
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
            // Check if Player can Jump
            var grounded = motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _timeSinceUngrounded < coyoteTime;
            if(grounded || canCoyoteJump && !_ungroundeDueToJump) // Can Jump
            {
                _requestedJump = false;
                _requestedCrouch = false;
                _requestedCrouchInAir = false;

                // Unstick the player from the ground.
                motor.ForceUnground(time: 0f);
                _ungroundeDueToJump = true;

                // Set Minimum Vertical Speed to the Jump Speed
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                // Add the difference in current and target vertical speed to the character's velocity
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else // Can't Jump
            {
                _timeSinceJumpRequest += deltaTime;

                // Defer the jump request until coyote time has passed
                var canJumpLater = _timeSinceJumpRequest < coyoteTime;
                _requestedJump = canJumpLater;
            }
        }
    }

    public void BlinkTeleport(Player player, PlayerCamera cam)
    {
        if (player.BlinkCharge < 100) { _currentBlinkTime = 0; return; }

        if (_requestedBlinkHeld && !_requestedBlinkRelease)
        {
            _currentBlinkTime += Time.deltaTime;

            //Test Blinking
            testBlinkEffect.SetActive(true);
        }
        else if (_requestedBlinkRelease)
        {
            //Test Blinking
            testBlinkEffect.SetActive(false);

            RaycastHit hit;
            float blinkDistance = baseBlinkDistance;
            if (_currentBlinkTime < .5f)
            {
                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, baseBlinkDistance))
                {
                    if (hit.distance < 2) { _currentBlinkTime = 0; return; }
                    blinkDistance = hit.distance - 1;
                }
            }
            else if (_currentBlinkTime >= .5f)
            {
                blinkDistance = Mathf.Lerp(baseBlinkDistance, maxBlinkDistance, (_currentBlinkTime - .5f) / maxBlinkTime);

                if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxBlinkDistance))
                {
                    if (hit.distance < 2) { _currentBlinkTime = 0; return; }
                    blinkDistance = hit.distance - 1;
                }
            }

            SetPosition(cam.transform.position + cam.transform.forward * blinkDistance);

            _currentBlinkTime = 0;

            player.BlinkCharge -= 100;

            if(player.Regen != null) { StopCoroutine(player.Regen); }
            player.Regen = StartCoroutine(player.ChargeBlink());
        }
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        // Crouch
        if(_requestedCrouch && _state.Stance is Stance.Stand)
        {
            // Shrink Character
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: 0
            );

            _state.Stance = Stance.Crouch;
        }
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        // Uncrouch
        if(!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            // Ceiling Detection
            bool stayCrouched = false;
            Collider[] hitColliders = Physics.OverlapSphere(ceilingCheck.position, ceilingCheckRadius);
            foreach (var hitCollider in hitColliders)
            {
                // Check if the hit object is not the player
                if (hitCollider.gameObject != this.gameObject)
                {
                    stayCrouched = true;
                }
            }

            if(!stayCrouched) // No Ceiling Detected
            {
                // Grow Character
                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: standHeight,
                    yOffset: 0
                );

                _state.Stance = Stance.Stand;
            }

        }

        // Update State to reflect relevant motor properties
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        // And update the _lastState to store the character state
        _lastState = _tempState;
    }
    public void PostGroundingUpdate(float deltaTime)
    {
        if(!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
        {
            _state.Stance = Stance.Crouch;
        }
    }

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {}

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {}

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
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

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
    }
}
