using System.Collections;
using UnityEngine;

public class MarbleController : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;

    // Constants

    private float MAX_SPEED = 5f; // Maximum speed the marble can reach
    private float ACCELERATION = 10f; // Force added to the marble to move it
    private float JUMP_FORCE = 5f; // Force added to the marble to make it jump

    // State Variables

    private bool canJump = true; // Self-explanatory ngl if you don't know what this does you may be stupid

    // Interior Values

    private Vector2 movementInput;

    public bool hasPowerup = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        // Marble Movement works by adding force to the Rigidbody2D if the marble is not at max speed already
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
        // Ground check, this variable resets every time the marble touches the ground
        // I've implemented it like this rather than directly doing a ground check to make it easier to add a double jump later if we decide to
        if (!canJump) return;

        // Reset the vertical velocity to 0 to make the jump feel snappier
        rb.linearVelocityY = 0;
        rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
        canJump = false;
    }

    // Collision Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with the map geometry
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }
    }


    public void ApplyPowerup(PowerupEffect powerup)
    {
        hasPowerup = true;
        StartCoroutine(PowerupCoroutine(powerup));
    }

    private IEnumerator PowerupCoroutine(PowerupEffect powerup)
    {
        powerup.Apply(this);

        // wait for effect to finish
        yield return new WaitForSeconds(powerup.duration);
        hasPowerup = false;
        powerup.Remove(this);
    }
}
