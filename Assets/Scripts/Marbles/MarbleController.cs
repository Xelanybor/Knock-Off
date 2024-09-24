using UnityEngine;

public class MarbleController : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;

    // Constants

    private float MAX_SPEED = 5f;
    private float ACCELERATION = 10f;
    private float JUMP_FORCE = 5f;

    // State Variables

    private bool canJump = true;

    // Interior Values

    private Vector2 movementInput;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (movementInput.x > 0 && rb.linearVelocity.x < MAX_SPEED)
        {
            rb.AddForce(Vector2.right * ACCELERATION);
        }
        else if (movementInput.x < 0 && rb.linearVelocity.x > -MAX_SPEED)
        {
            rb.AddForce(Vector2.left * ACCELERATION);
        }
    }

    // Player Movement Methods

    public void MovementInput(Vector2 input)
    {
        movementInput = input;
    }

    public void Jump()
    {
        rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
        canJump = false;
    }

    // Collision Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }

}
