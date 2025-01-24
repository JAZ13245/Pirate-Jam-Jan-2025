using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Movement and Look
    public Vector3 Move { get; private set; }
    public Vector2 Look { get; private set; }

    // Jumping
    public bool Jump { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleased { get; private set; }
    // Crouching
    public bool Crouch { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool CrouchReleased { get; private set; }
    // Shooting
    public bool Shoot { get; private set; }
    public bool ShootHeld { get; private set; }
    public bool ShootReleased { get; private set; }
    // Interacting
    public bool Interact { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractReleased { get; private set; }
    // Blinking
    public bool Blink { get; private set; }
    public bool BlinkHeld { get; private set; }
    public bool BlinkReleased { get; private set; }
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;
    private InputAction _shootAction;
    private InputAction _interactAction;
    private InputAction _blinkAction;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _playerInput = GetComponent<PlayerInput>();
        
        SetupInputActions();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInputs();
    }

    private void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        _crouchAction = _playerInput.actions["Crouch"];
        _shootAction = _playerInput.actions["Shoot"];
        _interactAction = _playerInput.actions["Interact"];
        _blinkAction = _playerInput.actions["Blink"];
    }

    private void UpdateInputs()
    {
        // Movement and Look inputs
        Move = _moveAction.ReadValue<Vector2>();
        Look = _lookAction.ReadValue<Vector2>();

        Jump = _jumpAction.WasPressedThisFrame();
        JumpHeld = _jumpAction.IsPressed();
        JumpReleased = _jumpAction.WasReleasedThisFrame();

        Crouch = _crouchAction.WasPressedThisFrame();
        CrouchHeld = _crouchAction.IsPressed();
        CrouchReleased = _crouchAction.WasReleasedThisFrame();

        Shoot = _shootAction.WasPressedThisFrame();
        ShootHeld = _shootAction.IsPressed();
        ShootReleased = _shootAction.WasReleasedThisFrame();

        Interact = _interactAction.WasPressedThisFrame();
        InteractHeld = _interactAction.IsPressed();
        InteractReleased = _interactAction.WasReleasedThisFrame();

        Blink = _blinkAction.WasPressedThisFrame();
        BlinkHeld = _blinkAction.IsPressed();
        BlinkReleased = _blinkAction.WasReleasedThisFrame();
    }
}