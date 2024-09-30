using UnityEngine;
using System.Collections;
public class PowerUpSpawner : MonoBehaviour
{
    // spawning time related     
    [SerializeField] private const int PowerUpThresh = 3;           // time until powerups start spawning (secs)
    [SerializeField] private const int MAX_POWERUP_TIME = 5;       // max time for powerup to spawn when timer is started
    [SerializeField] private const int PowerUpIdleTime = 5;         // time powerup stays on stage until despawning
    private float spawnTime = 0;
    private float idleTime = 0;
    private bool TimerOn = false;

    [SerializeField] private Transform[] spawnPoints;           // spawnpoints for powerups
    [SerializeField] private PowerupEffect[] powerUpSOList;     // powerup SO's

    private GameObject currentPowerUp;

    [SerializeField] private float blinkDuration = 8f;  // duration of the blinking effect before the object is destroyed
    [SerializeField] private float blinkSpeed = 3f;     // how fast the object fades in and out
    private bool blinking;
    private SpriteRenderer powerupRenderer;

    void Start()
    {
        currentPowerUp = null;
        powerupRenderer = null;
        blinking = false;
    }

    void Update()
    {
        // start spawning power ups after time threshold
        if (Time.time >= PowerUpThresh)
        {
            // check if powerup currently on stage, if not start timer and spawn one when timer finished
            if (currentPowerUp == null)
            {
                // check if timer started to spawn object after time threshold
                if (!TimerOn) { spawnTime = Random.Range(0, MAX_POWERUP_TIME); TimerOn = true; }    // create random time to spawn and start timer
                if (TimerOn) { spawnTime-=Time.deltaTime; } // count down timer
                if (spawnTime <= 0)     // spawn and reset timer
                { 
                    currentPowerUp = SpawnPowerUp();
                    powerupRenderer = currentPowerUp.GetComponent<SpriteRenderer>();
                    TimerOn = false; 
                }
            }
            // powerup on stage and not blinking (going to die)
            else if (!blinking)
            {
                idleTime += Time.deltaTime;
                if (idleTime >= PowerUpIdleTime) { StartCoroutine(animateThenDestroy(currentPowerUp)); blinking = true; }
            }
        }
    }
    
    // spawns a random powerup at a random location and returns the reference object for the powerup
    IEnumerator SpawnPowerUp()
    {
        Debug.Log("Powerup spawned");
        int randomSpawn = Random.Range(0, spawnPoints.Length);
        int randomPowerUp = Random.Range(0, powerUpSOList.Length);
        GameObject powerUpObject = Instantiate(powerUpSOList[randomPowerUp].prefab);
        powerUpObject.SetActive(true);
        powerUpObject.transform.position = spawnPoints[randomSpawn].position;

        idleTime = 0;

        return powerUpObject;
    }

    IEnumerator animateThenDestroy(GameObject currentPowerUp)
    {
        float elapsedTime = 0f;
        Color originalColour = powerupRenderer.color;

        Debug.Log("Powerup blinking.");
        // start blinking for a few seconds
        while (elapsedTime < blinkDuration)
        {
            if (currentPowerUp != null)
            {
                // calculate the transparency based on elapsed time and blink speed
                float alpha = (Mathf.Cos(elapsedTime * blinkSpeed * Mathf.PI) + 1f) / 2f;
                Color newColour = originalColour;
                newColour.a = alpha;
                powerupRenderer.color = newColour;

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            } else {
                blinking = false;
                currentPowerUp = null;
                yield break; // exit coroutine when player collides with powerup during blinking
            }
        }
        // don't destroy after blinking if player collided
        if (currentPowerUp != null)
        {
            powerupRenderer.color = originalColour;
            Debug.Log("Powerup destroyed");
            Destroy(currentPowerUp);
        }
        blinking = false;
    }
}