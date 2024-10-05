using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private MarbleController marblePrefab;
    private MarbleController marble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        marble = Instantiate(marblePrefab, new Vector3(-4, -1, 0), Quaternion.identity);
        marble.name = "Player Marble";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Player Input Methods

    // These methods are called by the Input System package when the player presses a key,
    // and then they call the corresponding method in the MarbleController class
    public void OnMove(InputAction.CallbackContext context)
    {
        marble.MovementInput(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            marble.Jump();
        }
    }

    public void OnFlick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            marble.StartChargingFlick();
        }
        else if (context.canceled)
        {
            marble.ReleaseFlick();
        }
    }


}
