using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using OpenCvSharp;
using UnityEngine.Rendering;
using System;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private bool crouchToggleable;
    [Header("Components")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    //[SerializeField] private CameraLean cameraLean;

    [SerializeField] private int _numberOfBlinks = 2;

    private int maxHealth = 100;
    private int currentHealth;

    public PlayerCharacter GetPlayerCharacter
    { get { return playerCharacter; } }

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

    GameManager gameManager;

    public Coroutine Regen = null;

    // Blink Detection Variables

    bool blink = false;
    bool blinkHeld = false;
    bool blinkRelased = false;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;

        gameManager = GameManager.Instance;

        Cursor.lockState = CursorLockMode.Locked;

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());

        cameraSpring.Initialize();
        //cameraLean.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        var deltaTime = Time.deltaTime;

        // Get Camera Input and Update its rotation and position.
        var cameraInput = new CameraInput{ Look = InputManager.Instance.Look };
        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());

        // Blink

        List<Vector2> points = gameManager.points;

        if (points.Count > 0 && (IsEyeClosed(DetectLeftEye(points)) || IsEyeClosed(DetectRightEye(points))))
        {
            if (!blink)
                blink = true;
            else
            {
                blink = false;
                blinkHeld = true;
            }
        }
        else if (blinkHeld)
        {
            blinkHeld = false;
            blinkRelased = true;
        }
        else
        {
            blinkRelased = false;
        }

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
            Blink = blink,
            BlinkHeld = blinkHeld,
            BlinkReleased = blinkRelased
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        playerCharacter.BlinkTeleport(this, playerCamera);

        // EDITOR ONLY: Allows Telporting the Player
#if UNITY_EDITOR
        if (Keyboard.current.tKey.wasPressedThisFrame)
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
        var cameraTarget = playerCharacter.GetCameraTarget();
        var state = playerCharacter.GetState();
        
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        /* Not Working / Looks Weird. May come back to this
        cameraLean.UpdateLean
        (
            deltaTime, 
            state.Stance is Stance.Slide, 
            state.Acceleration,
            cameraTarget.up
        );
        */
    }

    public void DamagePlayer(int damage)
    {
        currentHealth -= damage;
        //Debug.Log(currentHealth);
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

    public static List<Vector2> DetectLeftEye(List<Vector2> points)
    {
        //Points for the left eye are 37 to 42
        List<Vector2> eye = new List<Vector2>();
        for (int i = 36; i < 42; i++)
            eye.Add(points[i]);
        return eye;

    }

    public static List<Vector2> DetectRightEye(List<Vector2> points)
    {
        //Points for the left eye are 43 to 48
        List<Vector2> eye = new List<Vector2>();
        for (int i = 42; i < 48; i++)
            eye.Add(points[i]);
        return eye;

    }

    //Uses points given by the face detection to calculate when the eye is closed
    public float CalculateEAR(List<Vector2> eye)
    {
        float y1 = Vector3.Distance(eye[1], eye[5]);
        float y2 = Vector3.Distance(eye[2], eye[4]);

        float x1 = Vector3.Distance(eye[0], eye[3]);

        float EAR = (y1 + y2) / x1;

        return EAR;
    }

    public bool IsEyeClosed(List<Vector2> eye)
    {
        if (CalculateEAR(eye) <= 0.2) return true;
        return false;
    }

}
