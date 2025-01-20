using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    private static InputManager instance;
    public static InputManager Instance
    {
        get 
        { 
            return instance; 
        }
    }

    private void Awake() {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() {
        inputActions.Enable();
    }

    public Vector2 GetMovementInput()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }
}
