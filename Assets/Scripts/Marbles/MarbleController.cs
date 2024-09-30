using UnityEngine;

public class MarbleController : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    [SerializeField] private Shader flickTrajectoryShader;
    private ChargeIndicator flickChargeIndicator;

    // Constants

    private float MAX_SPEED = 5f; // Maximum speed the marble can reach
    private float ACCELERATION = 10f; // Force added to the marble to move it
    private float JUMP_FORCE = 5f; // Force added to the marble to make it jump

    private float [] FLICK_FORCE = {10f, 20f, 30f}; // Force added to the marble to make it flick, for different charge levels
    private float [] FLICK_CHARGE_TIMES = {0, 0.5f, 2.5f}; // Time needed to charge the flick for different charge levels
    private float [] FLICK_MOMENTUM = {10f, 20f, 30f}; // Momentum added to the marble when it flicks, for different charge levels
    private float FLICK_SLOWDOWN = 0.1f; // Slowdown applied to the flick force when the marble is charging it
    private float FLICK_BUFFER_TIME = 0.2f; // How long the flick direction is remembered after the joystick is released
    private float FLICK_MOVEMENT_LOCKOUT = 0.5f; // How long the marble is locked out of movement after a flick

    private float EQUAL_MOMENTUM_SCALE_FACTOR = 5f; // How much the marbles bounce back when they have equal momentum

    // State Variables

    private bool canJump = true; // Self-explanatory ngl if you don't know what this does you may be stupid
    private bool chargingFlick = false; // Whether the marble is currently charging a flick
    private bool lastMovementInputWasZero = false; // Whether the last movement input was the zero vector
    private bool resetMomentumNextUpdate = false; // Whether the marble's momentum should be reset on the next update

    // Interior Values

    private Vector2 movementInput;

    private float flickBufferTimer = 0;
    private float flickMovementLockoutTimer = 0;
    private int flickChargeLevel = -1; // The current charge level of the flick. -1 means the marble is not flicking
    private float flickChargeTimer = 0; // Timer for how long the flick has been charging

    private float momentum = 0;
    private Vector3 movementDirection = Vector3.zero; // Buffer for rb.linearVelocity, used for calculations as it's only updated once a frame



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        flickChargeIndicator = GetComponentInChildren<ChargeIndicator>();
        flickChargeIndicator.UpdateChargeValue(0); // Initialize the charge indicator to 0

        // Initialize the LineRenderer used to draw flick trajectory
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Debug.Log("LineRenderer: " + lineRenderer);
        
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

        // Reset momentum if needed
        // Done it this way because resetting momentum in OnCollisionEnter2D causes inconsistent behavior due to
        // each marble's collision methods being called asynchronously. Instead of resetting momentum in the collision
        // method, we set a flag to reset it in the next update
        if (resetMomentumNextUpdate)
        {
            momentum = 0;
            resetMomentumNextUpdate = false;
        }

        // Update the movement direction buffer
        movementDirection = rb.linearVelocity.normalized;

        // Timer before flick direction buffer is set to zero
        if (lastMovementInputWasZero)
        {
            if (flickBufferTimer <= 0)
            {
                movementInput = Vector2.zero;
            }
            else
            {
                flickBufferTimer -= Time.deltaTime;
            }
            
        }

        // Charge the flick if the marble is charging it and the charge level is not maxed out
        if (flickChargeLevel != -1)
        {
            if (flickChargeLevel < FLICK_FORCE.Length - 1)
            {
                // Update the charge indicator
                flickChargeIndicator.UpdateChargeValue(Mathf.InverseLerp(FLICK_CHARGE_TIMES[flickChargeLevel], FLICK_CHARGE_TIMES[flickChargeLevel + 1], flickChargeTimer));

                flickChargeTimer += Time.deltaTime;
                // Increase the flick charge level if the timer reaches the next level
                if (flickChargeTimer >= FLICK_CHARGE_TIMES[flickChargeLevel + 1])
                {
                    ++flickChargeLevel;
                }
            }
            else
            {
                flickChargeIndicator.UpdateChargeValue(0);
            }
        }

        // Timer before the marble can move again after a flick
        if (flickMovementLockoutTimer > 0)
        {
            flickMovementLockoutTimer -= Time.deltaTime;
        }

        if (chargingFlick) {

            DrawTrajectory(transform.position, movementInput, FLICK_FORCE[flickChargeLevel]);

        } else {

            // Clear the trajectory line when not charging a flick
            lineRenderer.positionCount = 0;

            // REGULAR MOVEMENT
            // Only allow movement if the marble is not locked out from flicking
            if (flickMovementLockoutTimer <= 0)
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

                // Moving resets momentum to zero
                momentum = 0;
            }
        }
    }

    // Player Movement Methods

    public void MovementInput(Vector2 input)
    {
        if (input != Vector2.zero)
        {
            movementInput = input.normalized;
            lastMovementInputWasZero = false;
            flickBufferTimer = FLICK_BUFFER_TIME;
        }
        else
        {
            // when a zero input is received, this boolean starts a
            // countdown before the direction buffer is also set to zero

            // this makes the flicking feel better by giving the player
            // a small window to release the joystick without losing the flick direction
            lastMovementInputWasZero = true;
        }
        
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
        flickChargeLevel = 0;
    }

    public void ReleaseFlick()
    {
        if (!chargingFlick) return;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
        rb.AddForce(movementInput * FLICK_FORCE[flickChargeLevel], ForceMode2D.Impulse);
        chargingFlick = false;

        // Reset the flick direction buffer
        movementInput = Vector2.zero;

        // Lock the marble out of movement for a short time after a flick
        flickMovementLockoutTimer = FLICK_MOVEMENT_LOCKOUT;

        // Set the marble's momentum (temporary value until we add flick charge levels)
        momentum = FLICK_MOMENTUM[flickChargeLevel];

        // Reset the flick charge
        flickChargeTimer = 0;
        flickChargeLevel = -1;
        flickChargeIndicator.UpdateChargeValue(0);
    }

    // Collision Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with the map geometry
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }

        // On collision with another marble
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform otherTransform = collision.gameObject.transform;
            MarbleController otherMarbleController = collision.gameObject.GetComponent<MarbleController>();
            // Get the marbles' effective momentum
            float effectiveMomentum = GetEffectiveMomentum(otherTransform.position);
            float enemyMomentum = otherMarbleController.GetEffectiveMomentum(transform.position);

            // Determine which marble is the attacker and which is the defender
            // The attacker is the marble with the higher effective momentum

            Debug.Log(name + " effective momentum: " + effectiveMomentum);

            if (effectiveMomentum > enemyMomentum)
            {
                // This marble is the attacker
                Debug.Log(name + " is the attacker");
                rb.linearVelocity = Vector2.zero;
                // float momentumDifference = enemyMomentum + effectiveMomentum;
                float force = 5f;
                Debug.Log(name + " force: " + force);
                rb.AddForce((transform.position - otherTransform.position).normalized * force, ForceMode2D.Impulse);
            }
            else if (effectiveMomentum == enemyMomentum)
            {
                // Both marbles are equally strong so they just bounce back
                Debug.Log(name + " has equal momentum");
                rb.linearVelocity = Vector2.zero;
                float force = 5f;
                Debug.Log(name + " force: " + force);
                rb.AddForce((transform.position - otherTransform.position).normalized * EQUAL_MOMENTUM_SCALE_FACTOR, ForceMode2D.Impulse);
            }
            else
            {
                // The other marble is the attacker
                Debug.Log(name + " is the defender");
                rb.linearVelocity = Vector2.zero;
                float momentumDifference = enemyMomentum + effectiveMomentum;
                float force = momentumDifference * 1.5f;
                Debug.Log(name + " force: " + force);
                rb.AddForce((transform.position - otherTransform.position).normalized * force, ForceMode2D.Impulse);
            }

            // Set a flag to reset momentum on the next update
            resetMomentumNextUpdate = true;
        }
    }

    public float GetEffectiveMomentum(Vector3 otherPosition)
    {
        return Mathf.Max(0f, momentum * Vector2.Dot(movementDirection, (otherPosition - transform.position).normalized));
    }

    // Drawing the flick trajectory

    private void DrawTrajectory(Vector2 start, Vector2 direction, float force)
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
