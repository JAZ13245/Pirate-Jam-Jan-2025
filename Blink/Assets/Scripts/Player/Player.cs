using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerCharacter.Intialize();
        playerCamera.Intialize(playerCharacter.GetCameraTarget());
    }

    // Update is called once per frame
    void Update()
    {
        // Get Camera Input and Update its rotation and position.
        var cameraInput = new CameraInput{ Look = InputManager.Instance.Look };
        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());

        // Get Character Input and Update it.
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = InputManager.Instance.Move,
            Jump = InputManager.Instance.Jump
        };
        playerCharacter.UpdateInput(characterInput);
    }
}
