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
    private bool usingCameraTracking = true;
    [Header("Components")]
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [Space]
    [SerializeField] private Animator knife;
    //[SerializeField] private CameraLean cameraLean;

    [Header("Blink Settings")]
    [SerializeField] private int _numberOfBlinks = 2;

    // Health variables
    private int maxHealth = 100;
    private int currentHealth;
    // 6500 is 6.5 seconds
    private float maxHealingTimer = 10;
    private float currentHealingTimer;

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

    private GameManager gameManager;

    public Coroutine Regen = null;

    // Blink Detection Variables

    bool blink = false;
    bool blinkHeld = false;
    bool blinkReleased = false;
    [Header("End Screen Settings")]
    [SerializeField] private EndScreenManager endScreenManager;
    private bool gamePaused = false;
    private bool playerDead = false;
    private bool playerWin = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentHealingTimer = maxHealingTimer;

        gameManager = GameManager.Instance;

        Cursor.lockState = CursorLockMode.Locked;

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());

        cameraSpring.Initialize();
        //cameraLean.Initialize();

        usingCameraTracking = gameManager.GetFaceCamEnable();
    }

    // Update is called once per frame
    void Update()
    {
        if(InputManager.Instance.Pause && !playerDead && !playerWin)
        {
            SetPause(!gamePaused);
        }

        if(gamePaused || playerDead || playerWin) return;

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
            blinkReleased = true;
        }
        else
        {
            blinkReleased = false;
        }

        // Get Character Input and Update it
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = InputManager.Instance.Move,
            Attack = InputManager.Instance.Shoot,
            Jump = InputManager.Instance.Jump,
            JumpHeld = InputManager.Instance.JumpHeld,
            Crouch = InputManager.Instance.Crouch,
            CrouchHeld = InputManager.Instance.CrouchHeld,
            crouchToggleable = crouchToggleable, 
            Blink = usingCameraTracking ? blink : InputManager.Instance.Blink,
            BlinkHeld = usingCameraTracking ? blinkHeld : InputManager.Instance.BlinkHeld,
            BlinkReleased = usingCameraTracking ? blinkReleased : InputManager.Instance.BlinkReleased
        };
        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);
        playerCharacter.BlinkTeleport(this, playerCamera);
        playerCharacter.UpdateAttack(knife);

        // System to repair health after not taking damage for a certain amount of time
        if (currentHealingTimer < maxHealingTimer)
            currentHealingTimer += Time.deltaTime;
        else if (currentHealth != maxHealth && currentHealth + 3 <= maxHealth)
            currentHealth += 3;
        else
            currentHealth = maxHealth;

        //Debug.Log("countdown: " + currentHealingTimer);

        /*
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
        */
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
        currentHealingTimer = 0;
        /*
        if (currentHealth <= 0)
            Death();
        */
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

    public void SetPause(bool pauseSet)
    {
        gamePaused = pauseSet;

        if(pauseSet){
            PauseGame(true);
            endScreenManager.showEndScreen(0);
        }else{
            PauseGame(false);
            endScreenManager.HideAllScreens();
        }
    }

    public void Win()
    {
        playerWin = true;
        PauseGame(true);
        endScreenManager.showEndScreen(1);
    }

    public void Death()
    {
        playerDead = true;
        PauseGame(true);
        endScreenManager.showEndScreen(1);
    }

    public void PauseGame(bool paused)
    {
        if(paused){
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }else{
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

}
