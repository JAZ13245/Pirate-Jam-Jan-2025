using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Movement and Look
    public Vector3 MoveInput { get; private set; }
    public Vector2 Look { get; private set; }

    // Jumping
    public bool Jump { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpReleased { get; private set; }

    // Shooting
    public bool Shoot { get; private set; }
    public bool ShootHeld { get; private set; }
    public bool ShootReleased { get; private set; }
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _shootAction;

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
        _shootAction = _playerInput.actions["Shoot"];
    }

    private void UpdateInputs()
    {
        // Movement and Look inputs
        MoveInput = _moveAction.ReadValue<Vector2>();
        Look = _lookAction.ReadValue<Vector2>();

        // Jump input states
        Jump = _jumpAction.WasPressedThisFrame();
        JumpHeld = _jumpAction.IsPressed();
        JumpReleased = _jumpAction.WasReleasedThisFrame();

        // Shoot input states
        Shoot = _shootAction.WasPressedThisFrame();
        ShootHeld = _shootAction.IsPressed();
        ShootReleased = _shootAction.WasReleasedThisFrame();
    }
}