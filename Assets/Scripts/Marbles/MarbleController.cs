using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.InputSystem;

public class MarbleController : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    [SerializeField] private Shader flickTrajectoryShader;
    private ChargeIndicator flickChargeIndicator;

    // Constants

    private float DASH_DISTANCE = 1f; // Distance the marble dashes
    private float DASH_COOLDOWN = 1f; // Multiplier for the time it takes to recharge the dash

    private float MAX_SPEED = 5f; // Maximum speed the marble can reach
    private float ACCELERATION = 10f; // Force added to the marble to move it
    private float JUMP_FORCE = 7f; // Force added to the marble to make it jump

    private float [] FLICK_FORCE = {10f, 20f, 30f}; // Force added to the marble to make it flick, for different charge levels
    private float [] FLICK_CHARGE_TIMES = {0, 0.5f, 2.5f}; // Time needed to charge the flick for different charge levels
    private float [] FLICK_MOMENTUM = {10f, 20f, 30f}; // Momentum added to the marble when it flicks, for different charge levels
    private float FLICK_SLOWDOWN = 0.1f; // Slowdown applied to the flick force when the marble is charging it
    private float FLICK_BUFFER_TIME = 0.2f; // How long the flick direction is remembered after the joystick is released
    private float FLICK_MOVEMENT_LOCKOUT = 0.5f; // How long the marble is locked out of movement after a flick
    private float[] FLICK_CHARGE_COSTS = { 0f, 1f, 2f, 3f};  // Energy cost per charge level
    public int FLICK_COUNT_MAX = 3;

    private float EQUAL_MOMENTUM_SCALE_FACTOR = 5f; // How much the marbles bounce back when they have equal momentum

    private float PERCENTAGE_SCALE = 2f; // The knockback multiplier at 100% damage
    private float DAMAGE_TO_PERCENTAGE = 3f; // Percentage gained per damage taken (momentum difference)

    // State Variables

    private bool canJump = true; // Self-explanatory ngl if you don't know what this does you may be stupid
    private bool chargingFlick = false; // Whether the marble is currently charging a flick
    private bool lastMovementInputWasZero = false; // Whether the last movement input was the zero vector
    private bool resetMomentumNextUpdate = false; // Whether the marble's momentum should be reset on the next update

    // Game Variables
    public int stockCount = 3; // Default stock count for the player.

    // Flick Counter variables
    private float flickCounter = 0f;            // Current flick energy
    public float flickCounterMax = 5f;         // Maximum flick energy
    public float flickCounterRegenRate = 1f;   // Energy regenerated per second

    // Events for communicating and updating flick bar
    public event EventHandler<OnUpdateEventArgs> OnEnergyUpdate;        // increment over time and decrement when release flick
    public event EventHandler<OnFlickBarCharge> OnCharge;               // when flicking held
    public class OnUpdateEventArgs : EventArgs
    {
        public float progressNormalized;
    }
    public class OnFlickBarCharge: EventArgs
    {
        public int chargeLevel;
    }


    // Interior Values

    private Vector2 movementInput;

    public bool hasPowerup = false;
    private float flickBufferTimer = 0;
    private float flickMovementLockoutTimer = 0;
    private int flickChargeLevel = -1; // The current charge level of the flick. -1 means the marble is not flicking
    private float flickChargeTimer = 0; // Timer for how long the flick has been charging

    private float momentum = 0;

    private Vector3 movementDirection = Vector3.zero; // Buffer for rb.linearVelocity, used for calculations as it's only updated once a frame

    private float percentage = 0f; // Percentage (knockback modifier)

    // MODIFIABLE STATS

    private Dictionary<string, float> stats = new Dictionary<string, float> {
        // Dashing
        {"DASH_DISTANCE", 1f},
        {"DASH_COOLDOWN_MULTIPLIER", 1f},

        // Movement
        {"MAX_SPEED_MULTIPLIER", 1f},
        {"ACCELERATION_MULTIPLIER", 1f},
        {"JUMP_FORCE_MULTIPLIER", 1f},

        // Flicks
        {"FLICK_CHARGE_SPEED_MULTIPLIER", 1f},
        {"FLICK_FORCE_MULTIPLIER", 1f},
        {"FLICK_MOMENTUM_MULTIPLIER", 1f},

        // Basic Stats
        {"KNOCKBACK_RESISTANCE", 0f},
        {"PERCENTAGE_DAMAGE_RESISTANCE", 0f},

        // Attacking
        {"EXTRA_KNOCKBACK_DEALT", 0f},
        {"EXTRA_PERCENTAGE_DAMAGE_DEALT", 0f},

    };

    // Set specific stats
    public void SetStats(Dictionary<string, float> newStats) {
        
        foreach (string key in newStats.Keys) {
            stats[key] = newStats[key];
        }

    }

    // Modify stats
    public void ModifyStats(Dictionary<string, float> statChanges) {
        
        foreach (string key in statChanges.Keys) {
            stats[key] += statChanges[key];
        }

    }

    public void UndoStatChanges(Dictionary<string, float> statChanges) {
        
        foreach (string key in statChanges.Keys) {
            stats[key] -= statChanges[key];
        }

    }

    // Get specific stat
    public float GetStat(string stat) {
        return stats[stat];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        flickChargeIndicator = GetComponentInChildren<ChargeIndicator>();
        flickChargeIndicator.UpdateChargeValue(0); // Initialize the charge indicator to 0

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

        // flick energy level updating
        if (flickCounter < flickCounterMax)
        {
            flickCounter += flickCounterRegenRate * Time.deltaTime; // increment
            if (flickCounter > flickCounterMax)
            {
                flickCounter = flickCounterMax;
            }

            // update UI
            OnEnergyUpdate?.Invoke(this, new OnUpdateEventArgs
            {
                progressNormalized = flickCounter / flickCounterMax
            });
        }

        // Charge the flick if the marble is charging it and the charge level is not maxed out
        if (flickChargeLevel != -1)
        {
            if (flickChargeLevel < FLICK_COUNT_MAX - 1)
            {
                // Update the charge indicator
                flickChargeIndicator.UpdateChargeValue(Mathf.InverseLerp(FLICK_CHARGE_TIMES[flickChargeLevel], FLICK_CHARGE_TIMES[flickChargeLevel + 1], flickChargeTimer));

                flickChargeTimer += Time.deltaTime * stats["FLICK_CHARGE_SPEED_MULTIPLIER"];
                // Increase the flick charge level if the timer reaches the next level
                if (flickChargeTimer >= FLICK_CHARGE_TIMES[flickChargeLevel + 1])
                {
                    ++flickChargeLevel;
                    // invoke update for flick charge level UI
                    OnCharge?.Invoke(this, new OnFlickBarCharge
                    {
                        chargeLevel = flickChargeLevel + 1
                    });
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
            DrawTrajectory(transform.position, movementInput, FLICK_FORCE[flickChargeLevel] * stats["FLICK_FORCE_MULTIPLIER"]);
        } else {

            // Clear the trajectory line when not charging a flick
            lineRenderer.positionCount = 0;

            // REGULAR MOVEMENT
            // Only allow movement if the marble is not locked out from flicking
            if (flickMovementLockoutTimer <= 0 && movementInput.x != 0)
            {
                // Marble Movement works by adding force to the Rigidbody2D if the marble is not at max speed already
                if (movementInput.x > 0 && rb.linearVelocity.x < MAX_SPEED * stats["MAX_SPEED_MULTIPLIER"])
                {
                    rb.AddForce(Vector2.right * ACCELERATION * stats["ACCELERATION_MULTIPLIER"]);
                }
                else if (movementInput.x < 0 && rb.linearVelocity.x > -MAX_SPEED * stats["MAX_SPEED_MULTIPLIER"])
                {
                    rb.AddForce(Vector2.left * ACCELERATION * stats["ACCELERATION_MULTIPLIER"]);
                }

                // Moving resets momentum to zero
                momentum = 0;
            }
        }
    }

    // Player Movement Methods

    public void MovementInput(Vector2 input)
    {

        // Don't allow movement if the marble is locked out from flicking
        if (!chargingFlick && flickMovementLockoutTimer > 0) return;

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
        rb.AddForce(Vector2.up * JUMP_FORCE * stats["JUMP_FORCE_MULTIPLIER"], ForceMode2D.Impulse);
        canJump = false;
    }

    public void StartChargingFlick()
    {
        if (flickCounter < FLICK_CHARGE_COSTS[1])
        {
            // if flick energy less than 1 charge can't start charging flick
            return;
        }
        chargingFlick = true;
        rb.linearVelocity *= FLICK_SLOWDOWN;
        rb.gravityScale = FLICK_SLOWDOWN * FLICK_SLOWDOWN;
        flickChargeLevel = 0;

        // invoke update for flick charge level UI
        OnCharge?.Invoke(this, new OnFlickBarCharge
        {
            chargeLevel = 1
        });
    }

    public void ReleaseFlick()
    {
        if (!chargingFlick) return;

        // decrement flickCounter by the cost of the charge
        float cost = FLICK_CHARGE_COSTS[flickChargeLevel+1];
        flickCounter -= cost;
        if (flickCounter < 0)
            flickCounter = 0;

        // update flickbar UI
        OnEnergyUpdate?.Invoke(this, new OnUpdateEventArgs
        {
            progressNormalized = flickCounter / flickCounterMax
        });

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
        rb.AddForce(movementInput * FLICK_FORCE[flickChargeLevel] * stats["FLICK_FORCE_MULTIPLIER"], ForceMode2D.Impulse);
        chargingFlick = false;

        // Reset the flick direction buffer
        movementInput = Vector2.zero;

        // Lock the marble out of movement for a short time after a flick
        flickMovementLockoutTimer = FLICK_MOVEMENT_LOCKOUT;

        // Set the marble's momentum (temporary value until we add flick charge levels)
        momentum = FLICK_MOMENTUM[flickChargeLevel] * stats["FLICK_MOMENTUM_MULTIPLIER"];

        // Reset the flick charge
        flickChargeTimer = 0;
        flickChargeLevel = -1;
        flickChargeIndicator.UpdateChargeValue(0);
        // invoke update for flick charge level UI
        OnCharge?.Invoke(this, new OnFlickBarCharge
        {   
            chargeLevel = 0
        });
    }

    // Collision Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with the map geometry
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }

        // On collision with a kill zone
        if (collision.gameObject.CompareTag("KillZone"))
        {
            // TODO: Code for respawning goes here !!!
        }

        // On collision with another marble
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform otherTransform = collision.gameObject.transform;
            MarbleController otherMarbleController = collision.gameObject.GetComponent<MarbleController>();
            // Get the marbles' effective momentum
            float effectiveMomentum = GetEffectiveMomentum(otherTransform.position);
            float enemyMomentum = otherMarbleController.GetEffectiveMomentum(transform.position);

            float force;

            float oldPercentage = percentage; // Save the old value for later calculations since the percentage might change if damage is taken

            // Determine which marble is the attacker and which is the defender
            // The attacker is the marble with the higher effective momentum

            if (effectiveMomentum > enemyMomentum)
            {
                // This marble is the attacker
                rb.linearVelocity = Vector2.zero;
                force = 5f;
            }
            else if (effectiveMomentum == enemyMomentum)
            {
                // Both marbles are equally strong so they just bounce back
                rb.linearVelocity = Vector2.zero;
                force = EQUAL_MOMENTUM_SCALE_FACTOR;
            }
            else
            {
                // The other marble is the attacker
                rb.linearVelocity = Vector2.zero;

                force = (enemyMomentum - effectiveMomentum) * 1.5f;

                // Apply damage to the marble
                percentage += (enemyMomentum - effectiveMomentum) * DAMAGE_TO_PERCENTAGE * (1 + stats["EXTRA_PERCENTAGE_DAMAGE_DEALT"] + otherMarbleController.GetStat("EXTRA_PERCENTAGE_DAMAGE_DEALT"));
            }

            // Apply the force
            force *= Mathf.Pow(PERCENTAGE_SCALE, oldPercentage / 100f); // Apply percentage modifier
            force /= 1 + stats["KNOCKBACK_RESISTANCE"] - otherMarbleController.GetStat("EXTRA_KNOCKBACK_DEALT"); // Apply knockback resistance
            rb.AddForce((transform.position - otherTransform.position).normalized * force, ForceMode2D.Impulse);

            // Set a flag to reset momentum on the next update
            resetMomentumNextUpdate = true;
        }
    }

    public float GetEffectiveMomentum(Vector3 otherPosition)
    {
        return Mathf.Max(0f, momentum * Vector2.Dot(movementDirection, (otherPosition - transform.position).normalized));
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

    // Getters and Setters

    public float GetMomentum()
    {
        return momentum;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        this.MovementInput(context.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            this.Jump();
        }
    }

    public void OnFlick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            this.StartChargingFlick();
        }
        else if (context.canceled)
        {
            this.ReleaseFlick();
        }
    }

}
