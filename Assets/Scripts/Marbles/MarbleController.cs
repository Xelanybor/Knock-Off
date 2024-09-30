using UnityEngine;

public class MarbleController : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    [SerializeField] private Shader flickTrajectoryShader;

    // Constants

    private float MAX_SPEED = 5f; // Maximum speed the marble can reach
    private float ACCELERATION = 10f; // Force added to the marble to move it
    private float JUMP_FORCE = 5f; // Force added to the marble to make it jump

    private float FLICK_FORCE = 20f; // Force added to the marble to make it flick
    private float FLICK_SLOWDOWN = 0.1f; // Slowdown applied to the flick force when the marble is charging it

    // State Variables

    private bool canJump = true; // Self-explanatory ngl if you don't know what this does you may be stupid
    private bool chargingFlick = false; // Whether the marble is currently charging a flick

    // Interior Values

    private Vector2 movementInput;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Initialize the LineRenderer used to draw flick trajectory
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        Material trajectoryMaterial = new Material(flickTrajectoryShader);
        lineRenderer.material = trajectoryMaterial;
        // lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.textureMode = LineTextureMode.Tile;

        Gradient trajectoryGradient = new Gradient();
        trajectoryGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = trajectoryGradient;

        // Make the circles 2/5 of the size
        lineRenderer.widthMultiplier = 0.4f;
        lineRenderer.textureScale = new Vector2(2.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {

        if (chargingFlick) {

            drawTrajectory(transform.position, movementInput, FLICK_FORCE);

        } else {

            // Clear the trajectory line when not charging a flick
            lineRenderer.positionCount = 0;

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
    }

    // Player Movement Methods

    public void MovementInput(Vector2 input)
    {
        movementInput = input.normalized;
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

    public void StartChargingFlick()
    {
        chargingFlick = true;
        rb.linearVelocity *= FLICK_SLOWDOWN;
        rb.gravityScale = FLICK_SLOWDOWN * FLICK_SLOWDOWN;
    }

    public void ReleaseFlick()
    {
        if (!chargingFlick) return;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
        rb.AddForce(movementInput * FLICK_FORCE, ForceMode2D.Impulse);
        chargingFlick = false;
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

    // Drawing the flick trajectory

    private void drawTrajectory(Vector2 start, Vector2 direction, float force)
    {

        // Clear line if not pointing in a direction
        if (direction == Vector2.zero) {
            lineRenderer.positionCount = 0;
            return;
        }

        // Render the line by calculating the trajectory of the flick
        int segments = 10;
        float timeStep = 0.01f;
        float time = 0;

        Vector2 velocity = direction * force;
        Vector2 position = start;

        lineRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            lineRenderer.SetPosition(i, position);
            position += velocity * time + 0.5f * Physics2D.gravity * time * time;
            velocity += Physics2D.gravity * time;
            time += timeStep;
        }
    }

}
