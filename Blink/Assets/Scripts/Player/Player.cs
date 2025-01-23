using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool crouchToggleable;
    [Header("Components")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;

    [SerializeField] private int _numberOfBlinks = 2;
    public int NumberOfBlinks { get { return _numberOfBlinks; } }
    
    private int blinkCharge = 200;
    public int BlinkCharge { 
        get { return blinkCharge; } 
        set {
            if (blinkCharge == value) return;
            blinkCharge = value;
            if (OnVariableChange != null)
                OnVariableChange(blinkCharge);
        }
    }
    public delegate void OnVariableChangeDelegate(int newVal);
    public event OnVariableChangeDelegate OnVariableChange;

    [SerializeField] private float _blinkChargeDuration = 3f;

    public Coroutine Regen = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerCharacter.Intialize();
        playerCamera.Intialize(playerCharacter.GetCameraTarget());

        cameraSpring.Intialize();
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;

        // Get Camera Input and Update its rotation and position.
        var cameraInput = new CameraInput{ Look = InputManager.Instance.Look };
        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());

        // Get Character Input and Update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = InputManager.Instance.Move,
            Jump = InputManager.Instance.Jump,
            JumpHeld = InputManager.Instance.JumpHeld,
            Crouch = InputManager.Instance.Crouch,
            CrouchHeld = InputManager.Instance.CrouchHeld,
            crouchToggleable = crouchToggleable, 
            Blink = InputManager.Instance.Blink,
            BlinkHeld = InputManager.Instance.BlinkHeld,
            BlinkReleased = InputManager.Instance.BlinkReleased
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        playerCharacter.BlinkTeleport(this, playerCamera);

        // EDITOR ONLY: Allows Telporting the Player
        #if UNITY_EDITOR
        if(Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if(Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }
        #endif
    }

    private void LateUpdate() {
        var deltaTime = Time.deltaTime;;
        cameraSpring.UpdateSpring(deltaTime);
    }

    public IEnumerator ChargeBlink()
    {
        yield return new WaitForSeconds(.5f);

        while (BlinkCharge < _numberOfBlinks * 100)
        {
            BlinkCharge += (int)((_numberOfBlinks * 100) / (_blinkChargeDuration*10));
            yield return new WaitForSeconds(.1f);
        }

        Regen = null;
    }

    public void Teleport(Vector3 position, bool killVelocity = true)
    {
        playerCharacter.SetPosition(position, killVelocity);
    }
}
