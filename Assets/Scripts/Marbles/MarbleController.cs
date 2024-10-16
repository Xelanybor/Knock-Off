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
    [SerializeField] private MetalMarbleSprite metalMarbleSprite;

    // Constants

    private float DASH_DISTANCE = 2f; // Distance the marble dashes
    private float DASH_TIME = 0.1f; // Time the dash lasts

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
    public int characterIndex = 0;
    public int spriteIndex = 0;
    public int stockCount
    {
        get { return _stockCount; }
        set
        {
            _stockCount = value;
            OnStockChange?.Invoke(this, new OnStockChangeArg
            {
                stockCount = _stockCount
            });
        }
    }

    private int _stockCount = 3; // Backing field for stock count
    public bool ready = true;
    public bool match_can_begin = false;
    public bool start_match = false;
    public bool dead = false;
    public bool isWinner = false;

    public string selectedMap = "Random";
    public bool voted = false;


    // Flick Counter variables
    public float flickCounter = 0f;            // Current flick energy
    public float flickCounterMax = 5f;         // Maximum flick energy
    public float flickCounterRegenRate = 1.25f;   // Energy regenerated per second

    // Events for communicating and updating flick bar
    public event EventHandler<OnUpdateEventArgs> OnEnergyUpdate;        // increment over time and decrement when release flick
    public event EventHandler<OnFlickBarCharge> OnCharge;               // when flicking held

    public event EventHandler<OnApplyPowerUp> PickUpPowerUp;

    public event EventHandler<OnAddBot> AddBot;
    public event EventHandler<OnRemoveBot> RemoveBot;

    public class OnAddBot : EventArgs
    {
        public bool addBot;
    }

    public class OnRemoveBot : EventArgs
    {
        public bool removeBot;
    }

    public class OnUpdateEventArgs : EventArgs
    {
        public float progressNormalized;
    }
    public class OnFlickBarCharge: EventArgs
    {
        public int chargeLevel;
    }

    public class OnApplyPowerUp : EventArgs
    {
        public PowerupEffect powerup;
    }


    // Interior Values

    private Vector2 movementInput;
    private float flickBufferTimer = 0;
    private float flickMovementLockoutTimer = 0;
    private int flickChargeLevel = -1; // The current charge level of the flick. -1 means the marble is not flicking
    private float flickChargeTimer = 0; // Timer for how long the flick has been charging

    private float dashTimer = 0; // Timer for how long the marble has been dashing
    private bool canDash = true; // If the marble can dash
    private Vector3 dashVelocity = Vector3.zero; // Velocity of the marble during the dash

    private float momentum = 0;

    private Vector3 movementDirection = Vector3.zero; // Buffer for rb.linearVelocity, used for calculations as it's only updated once a frame


    private float _percentage = 0f; // Backing Field for percentage

    private float percentage
    {
        get { return _percentage; }
        set
        {
            _percentage = value;
            OnPercentageChange?.Invoke(this, new OnPercentageChangeArg
            {
                percentage = _percentage
            });
        }
    }
    // Percentage Event
    public event EventHandler<OnPercentageChangeArg> OnPercentageChange;
    public class OnPercentageChangeArg : EventArgs
    {
        public float percentage;
    }

    // Stock Event
    public event EventHandler<OnStockChangeArg> OnStockChange;
    public class OnStockChangeArg : EventArgs
    {
        public int stockCount;
    }

    // powerups
    public bool hasPowerup = false;

    public class OnPowerUpStatus : EventArgs
    {
        public bool hasPowerup;
    }
    public event EventHandler<OnPowerUpStatus> onPowerUpBool;

    private Coroutine activePowerupCoroutine;
    private PowerupEffect currentPowerup;

    // Audio
    [SerializeField] private AudioClip flickSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip chargeSound;
    private AudioSource chargeSource;

    //[SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip[] mildCollisionSounds;
    [SerializeField] private AudioClip hardCollisionSound;
    [SerializeField] private AudioClip[] damageVoiceLines;
    [SerializeField] private AudioClip[] killVoiceLines;

    // collision with ground objects
    [SerializeField] private AudioClip groundCollision;

    [SerializeField] private AudioClip[] deathSounds;
    [SerializeField] private AudioClip[] tauntSounds;
    [SerializeField] private AudioClip gameOverSound;
    // particles and flash effects
    [SerializeField] private ParticleSystem damageParticles;
    private ParticleSystem damageParticleInstance;
    private DamageFlash damageFlash;


    public event EventHandler OnDamageFaceUpdate;

    // MODIFIABLE STATS

    private Dictionary<string, float> stats = new Dictionary<string, float> {
        // Dashing
        {"DASH_TIME_MULTIPLIER", 1f},
        {"DASH_DISTANCE_MULTIPLIER", 1f},

        // Movement
        {"MAX_SPEED_MULTIPLIER", 1f},
        {"ACCELERATION_MULTIPLIER", 1f},
        {"JUMP_FORCE_MULTIPLIER", 1f},

        // Flicks
        {"FLICK_CHARGE_SPEED_MULTIPLIER", 1f},
        {"FLICK_REGEN_RATE_MULTIPLIER", 1f},
        {"FLICK_FORCE_MULTIPLIER", 1f},
        {"FLICK_MOMENTUM_MULTIPLIER", 1f},

        // Basic Stats
        {"KNOCKBACK_RESISTANCE", 0f},
        {"PERCENTAGE_DAMAGE_RESISTANCE", 0f},

        // Attacking
        {"EXTRA_KNOCKBACK_DEALT", 0f},
        {"EXTRA_PERCENTAGE_DAMAGE_DEALT", 0f},

        // ignore
        {"EXTRA_FLICK", 0f}

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

    // take string for marble name, set stats accordingly
    public void SetMarbleType(string charName)
    {
        charName = charName.ToUpper();
        switch (charName)
        {
            case "CAT":
                SetStats(new Dictionary<string, float> {
                    {"DASH_TIME_MULTIPLIER", 0.9f},     // + quicker dash
                    {"DASH_DISTANCE_MULTIPLIER", 1.1f}, // + further dash
                    {"KNOCKBACK_RESISTANCE", -0.2f},     // - less knockback resistance
                    });
                break;

            case "SWIRLY":
                SetStats(new Dictionary<string, float> {
                    {"MAX_SPEED_MULTIPLIER", 1.4f},            // + higher max speed
                    {"ACCELERATION_MULTIPLIER", 1.2f},         // + more acc
                    {"FLICK_REGEN_RATE_MULTIPLIER", 0.75f},  // - less charging
                    });
                break;

            case "STARRY":
                SetStats(new Dictionary<string, float> {
                    {"FLICK_CHARGE_SPEED_MULTIPLIER", 1.25f},      // + flick charge faster
                    {"FLICK_REGEN_RATE_MULTIPLIER", 1.25f},        // + flick regen faster
                    {"EXTRA_KNOCKBACK_DEALT", -0.2f},               // - less knockback dealt
                    {"EXTRA_PERCENTAGE_DAMAGE_DEALT", -0.2f}});     // less percentage dealt
                break;

            case "RUSTY":
                SetStats(new Dictionary<string, float> {
                    {"ACCELERATION_MULTIPLIER", 0.8f},        // - acceleration
                    {"PERCENTAGE_DAMAGE_RESISTANCE", 0.25f},   // + damage resistance
                    }); 
                break;
        }
    }

    // Get specific stat
    public float GetStat(string stat) {
        return stats[stat];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Register the events

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

        damageFlash = GetComponent<DamageFlash>();
    }

    // Update is called once per frame
    void Update()
    {

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Extra check for the marble being dead
        // In case the marble quantum tunnels through the kill collider
        if (sceneName == "Arena" && (Mathf.Abs(transform.position.x) > 80 || Mathf.Abs(transform.position.y) > 50))
        {
            if (GameManager.Instance.currentState == GameManager.GameState.Game)
            {
                Die();
            }
        }

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
            flickCounter += flickCounterRegenRate * stats["FLICK_REGEN_RATE_MULTIPLIER"] * Time.deltaTime; // increment
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

        // DASHING

        // Timer before the marble can move again after a flick
        if (flickMovementLockoutTimer > 0)
        {
            flickMovementLockoutTimer -= Time.deltaTime;
        }

        // Time that the marble is busy dashing for
        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            rb.linearVelocity = dashVelocity;

            if (dashTimer <= 0)
            {
                rb.gravityScale = 1;
                rb.linearVelocity /= 2 * stats["DASH_TIME_MULTIPLIER"];
            }

        }

        // REGULAR MOVEMENT

        if (chargingFlick) {
            DrawTrajectory(transform.position, movementInput, FLICK_FORCE[flickChargeLevel] * stats["FLICK_FORCE_MULTIPLIER"]);
        } else {

            // Clear the trajectory line when not charging a flick
            lineRenderer.positionCount = 0;

            // REGULAR MOVEMENT
            // Only allow movement if the marble is not locked out from flicking
            if (flickMovementLockoutTimer <= 0 && movementInput.x != 0 && dashTimer <= 0)
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

        // Can't jump while charging a flick
        if (chargingFlick) return;

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
        // start audio
        if (chargeSource == null)
        {
            chargeSource = SoundFXManager.Instance.PlaySoundFXClip(chargeSound, gameObject.transform, 0.1f);
        }
        chargingFlick = true;
        momentum = 0;
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
        SoundFXManager.Instance.PlaySoundFXClip(flickSound, gameObject.transform, 0.3f);

        // decrement flickCounter by the cost of the charge
        float cost = FLICK_CHARGE_COSTS[flickChargeLevel+1];
        flickCounter -= cost;
        if (flickCounter < 0)
            flickCounter = 0;


        rb.linearVelocity = Vector2.zero;
        rb.AddForce(movementInput * FLICK_FORCE[flickChargeLevel] * stats["FLICK_FORCE_MULTIPLIER"], ForceMode2D.Impulse);

        // Reset the flick direction buffer
        movementInput = Vector2.zero;

        // Lock the marble out of movement for a short time after a flick
        flickMovementLockoutTimer = FLICK_MOVEMENT_LOCKOUT;

        // Set the marble's momentum (temporary value until we add flick charge levels)
        momentum = FLICK_MOMENTUM[flickChargeLevel] * stats["FLICK_MOMENTUM_MULTIPLIER"];

        StopChargingFlick();
    }

    public void StopChargingFlick()
    {

        // Method called both when a flick is released and when a flick is interrupted

        if (!chargingFlick) return;
        chargingFlick = false;
        rb.gravityScale = 1;

        // stop audio
        if (chargeSource != null)
        {
            SoundFXManager.Instance.StopSound(chargeSource);
        }

        // update flickbar UI
        OnEnergyUpdate?.Invoke(this, new OnUpdateEventArgs
        {
            progressNormalized = flickCounter / flickCounterMax
        });

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

    public void Dash()
    {
        if (!canDash) return;

        dashTimer = DASH_TIME * stats["DASH_TIME_MULTIPLIER"];
        canDash = false;

        // Calculate the dash velocity (movementInput is already normalized)
        dashVelocity = DASH_DISTANCE * stats["DASH_DISTANCE_MULTIPLIER"] * movementInput / dashTimer;
        rb.gravityScale = 0;
        rb.linearVelocity = dashVelocity;
        momentum = 0;

    }

    private void PlayTauntSound()
    {
        SoundFXManager.Instance.PlayRandomSoundFXClip(tauntSounds, gameObject.transform, 0.4f);
    }

    private void Die()
    {
        if (this.stockCount <= 0)
        {
            return;
        }
        this.stockCount = this.stockCount - 1;
            this.dead = true;

            // play marble death sound
            SoundFXManager.Instance.PlayRandomSoundFXClip(deathSounds, gameObject.transform, 0.5f);

            // play opponent marble mock sound
            Invoke("PlayTauntSound", 0.2f);
            // reset powerup if had one
            if (hasPowerup)
            {
                if (activePowerupCoroutine != null)
                {
                    StopCoroutine(activePowerupCoroutine);
                    activePowerupCoroutine = null;
                }
                metalMarbleSprite.Disable();
                transform.localScale = new Vector3(1f, 1f, 1f);

            // undo the state changes
            currentPowerup.Remove(this);
                // reset state variables
                hasPowerup = false;
                onPowerUpBool?.Invoke(this, new OnPowerUpStatus
                {
                    hasPowerup = false
                });
            currentPowerup = null;
            }
    }

    // Collision Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with the map geometry
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
            canDash = true;
            SoundFXManager.Instance.PlaySoundFXClip(groundCollision, gameObject.transform, 0.2f);
        }

        // On collision with a kill zone
        if (collision.gameObject.CompareTag("KillZone"))
        {
            Die();
        }

        // On collision with another marble
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform otherTransform = collision.gameObject.transform;
            MarbleController otherMarbleController = collision.gameObject.GetComponent<MarbleController>();
            // Get the marbles' effective momentumscale
            float effectiveMomentum = GetEffectiveMomentum(otherTransform.position);
            float enemyMomentum = otherMarbleController.GetEffectiveMomentum(transform.position);

            Vector2 attackDirection = (transform.position - otherTransform.position).normalized;

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

                if (chargingFlick)
                {
                    // Marble gets interrupted if charging a flick
                    StopChargingFlick();
                }

                // The other marble is the attacker
                rb.linearVelocity = Vector2.zero;

                force = (enemyMomentum - effectiveMomentum) * 1.5f * Mathf.Pow(PERCENTAGE_SCALE, oldPercentage / 100f);

                // Apply damage to the marble
                float damage = (enemyMomentum - effectiveMomentum) * DAMAGE_TO_PERCENTAGE * (1 - stats["PERCENTAGE_DAMAGE_RESISTANCE"] + otherMarbleController.GetStat("EXTRA_PERCENTAGE_DAMAGE_DEALT"));
                percentage += damage;
                // choose collision sound effect to apply
                if (damage > 0 && damage <= 50)
                {
                    SoundFXManager.Instance.PlayRandomSoundFXClip(mildCollisionSounds, gameObject.transform, 0.5f);
                }
                else
                {
                    SoundFXManager.Instance.PlayRandomSoundFXClip(mildCollisionSounds, gameObject.transform, 0.4f);
                    SoundFXManager.Instance.PlaySoundFXClip(hardCollisionSound, gameObject.transform, 0.8f);
                }
                OnDamageFaceUpdate?.Invoke(this, EventArgs.Empty);
                SoundFXManager.Instance.PlayRandomSoundFXClip(damageVoiceLines, gameObject.transform, 0.35f);
                // particles
                SpawnDamageParticles(attackDirection);
                damageFlash.CallDamageFlash();
                
            }


            // Apply the force
            force /= 1 + stats["KNOCKBACK_RESISTANCE"] - otherMarbleController.GetStat("EXTRA_KNOCKBACK_DEALT"); // Apply knockback resistance
            // force = Mathf.Clamp(force, 0, 100f);
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
        // set variables
        hasPowerup = true;
        onPowerUpBool?.Invoke(this, new OnPowerUpStatus
        {
            hasPowerup = true
        });
        currentPowerup = powerup;

        // Yes the metal marble name has a spelling mistake, but I'm too afraid to rename it now
        if (powerup.name == "MetaMarble")
        {
            metalMarbleSprite.Enable();
        }

        if (powerup.name == "BigMarble")
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        PickUpPowerUp?.Invoke(this, new OnApplyPowerUp
        {
            powerup = powerup
        });

        // stop existing
        if (activePowerupCoroutine != null)
        {
            StopCoroutine(activePowerupCoroutine);
        }

        // start the new power-up coroutine
        activePowerupCoroutine = StartCoroutine(PowerupCoroutine(powerup));
    }

    private IEnumerator PowerupCoroutine(PowerupEffect powerup)
    {
        powerup.Apply(this);

        // wait for effect to finish
        yield return new WaitForSeconds(powerup.duration);
        hasPowerup = false;
        onPowerUpBool?.Invoke(this, new OnPowerUpStatus
        {
            hasPowerup = false
        });
        metalMarbleSprite.Disable();
        transform.localScale = new Vector3(1f, 1f, 1f);
        powerup.Remove(this);
        currentPowerup = null;
        activePowerupCoroutine = null;
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

    public bool CanDash()
    {
        return canDash;
    }

    public float GetMomentum()
    {
        return momentum;
    }

    public float GetPercentage()
    {
        return percentage;
    }

    public void ResetPercentage()
    {
        percentage = 0;
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
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            this.Dash();
            SoundFXManager.Instance.PlaySoundFXClip(dashSound, gameObject.transform, 0.3f);
        }
    }
    
    public void OnChangeSkin(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (!ready && context.started && input.x != 0)
        {
            GameManager.Instance.ChangeMarbleCharacter(this, input);
        }
    }

    public void OnChangeMapVote(InputAction.CallbackContext context)
    {
        if (this.voted) return;
        Vector2 input = context.ReadValue<Vector2>();
        if (context.started && input.x != 0)
        {
            GameManager.Instance.MoveMarbleOverMap(this, input);
        }
    }

    public void OnSubmitVote(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.ConfirmVote(this);
        }
    }

    public void OnAcceptWin(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.AcceptWin(this);
        }
    }

    public void OnRequestAddBot(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Lobby) return;
        if (context.started)
        {
            AddBot?.Invoke(this, new OnAddBot
            {
                addBot = true
            });
        }
    }

    public void OnRequestRemoveBot(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Lobby) return;
        if (context.started)
        {
            RemoveBot?.Invoke(this, new OnRemoveBot
            {
                removeBot = true
            });
        }
    }

    public void OnAmReady(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ready = true;
            if (match_can_begin)
            {
                start_match = true;
            }
        }
    }

    public void OnAmNotReady(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ready = false;
            start_match = false;
            match_can_begin = false;
        }
    }

    public void ReturnToLobby(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameManager.Instance.GoBackToLobby(this);
        }
    }

    public void SpawnDamageParticles(Vector2 attackDirection)
    {
        Quaternion spawnRotation = Quaternion.FromToRotation(Vector2.right, attackDirection);
        damageParticleInstance = Instantiate(damageParticles, gameObject.transform.position, spawnRotation);
    }

}
