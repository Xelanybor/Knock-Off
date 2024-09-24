using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private MarbleController marblePrefab;
    private MarbleController marble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        marble = Instantiate(marblePrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Player Input Methods

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


}
