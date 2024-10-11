using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PowerUpSpawner : MonoBehaviour
{
    // spawning time related     
    [SerializeField] private const int PowerUpThresh = 3;           // time until powerups start spawning (secs)
    [SerializeField] private const int MAX_POWERUP_TIME = 8;       // max time for powerup to spawn when timer is started
    [SerializeField] private const int MIN_POWERUP_TIME = 5;

    [SerializeField] private const int PowerUpIdleTime = 5;         // time powerup stays on stage until despawning
    private float spawnTime = 0;
    private float idleTime = 0;
    private bool TimerOn = false;

    private List<Vector3> spawnPoints;           // spawnpoints for powerups
    [SerializeField] private PowerupEffect[] powerUpSOList;     // powerup SO's

    private GameObject currentPowerUp;

    [SerializeField] private float blinkDestroyDuration = 8f;  // duration of the blinking effect before the object is destroyed
    [SerializeField] private float blinkSpawnDuration = 0.1f;
    //[SerializeField] private float blinkSpeed = 3f;     // how fast the object fades in and out
    private bool blinking;
    private SpriteRenderer powerupRenderer;

    [SerializeField] private AnimationCurve fadeInCurve;
    [SerializeField] private AnimationCurve fadeOutCurve;
    [SerializeField] private AudioClip[] FadeInSounds;
    [SerializeField] private AudioClip FadeOutSound;
    private AudioSource fadeOutSource;  // for killing the fade out sound if collected
    [SerializeField] private AudioClip PowerUpDie;
    void Start()
    {
        currentPowerUp = null;
        powerupRenderer = null;
        blinking = false;
        Map map = FindFirstObjectByType<Map>();
        spawnPoints = map.getSpawnPoints();
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
                if (!TimerOn) { spawnTime = Random.Range(MIN_POWERUP_TIME, MAX_POWERUP_TIME+1); TimerOn = true; }    // create random time to spawn and start timer
                if (TimerOn) { spawnTime -= Time.deltaTime; } // count down timer
                if (spawnTime <= 0)     // spawn and reset timer
                {
                    StartCoroutine(SpawnPowerUp());
                    TimerOn = false;
                }
            }
            // powerup on stage and not blinking (going to die)
            else if (!blinking)
            {
                idleTime += Time.deltaTime;
                // Debug.Log(idleTime);
                if (idleTime >= PowerUpIdleTime) { StartCoroutine(animateThenDestroy(currentPowerUp)); blinking = true; }
            }
        }
    }

    // spawns a random powerup at a random location and returns the reference object for the powerup
    IEnumerator SpawnPowerUp()
    {
        int randomSpawn = Random.Range(0, spawnPoints.Count);
        int randomPowerUp = Random.Range(0, powerUpSOList.Length);
        idleTime = 0;
        blinking = true;

        GameObject powerUpObject = Instantiate(powerUpSOList[randomPowerUp].prefab);
        currentPowerUp = powerUpObject;
        powerupRenderer = currentPowerUp.GetComponent<SpriteRenderer>();

        powerUpObject.SetActive(true);
        powerUpObject.transform.position = spawnPoints[randomSpawn];

        yield return StartCoroutine(animateSpawn(powerUpObject));
        powerUpObject.GetComponent<BoxCollider2D>().enabled = true; // enable collider after spawn blinkiing
    }

    IEnumerator animateSpawn(GameObject powerUpObject)
    {
        float elapsedTime = 0f;
        Color originalColour = powerupRenderer.color + new Color(0f, 0f, 0f, 1f);
        SoundFXManager.Instance.PlayRandomSoundFXClip(FadeInSounds, gameObject.transform, 0.35f);

        // Debug.Log("Powerup spawning...");

        while (elapsedTime < blinkSpawnDuration)
        {
            float normalizedTime = elapsedTime / blinkSpawnDuration;

            float alpha = Mathf.Sin(normalizedTime * Mathf.PI / 2f);

            Color newColour = originalColour;
            newColour.a = alpha;
            powerupRenderer.color = newColour;

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final color is set
        powerupRenderer.color = originalColour;
        blinking = false;
    }

    IEnumerator animateThenDestroy(GameObject currentPowerUp)
    {
        float elapsedTime = 0f;
        Color originalColour = powerupRenderer.color;
        // Debug.Log("Powerup blinking.");
        // start blinking for a few seconds
        fadeOutSource = SoundFXManager.Instance.PlaySoundFXClip(FadeOutSound, gameObject.transform, 0.2f);
        while (elapsedTime < blinkDestroyDuration)
        {
            if (currentPowerUp != null)
            {
                float normalizedTime = elapsedTime / blinkDestroyDuration;  // (0 to 1)

                // Get amplitude from the AnimationCurve
                float envelope = fadeOutCurve.Evaluate(normalizedTime);

                // Oscillation using cosine function
                float oscillation = Mathf.Cos(2f * Mathf.PI * 4 * envelope * normalizedTime);

                // Combine envelope and oscillation
                //float alpha = envelope * Mathf.Abs(oscillation);
                float alpha = Mathf.Abs(oscillation);

                Color newColour = originalColour;
                newColour.a = alpha;
                powerupRenderer.color = newColour;

                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            else
            {
                blinking = false;
                currentPowerUp = null;
                SoundFXManager.Instance.StopSound(fadeOutSource);
                fadeOutSource = null;
                yield break; // exit coroutine when player collides with powerup during blinking
            }
        }
        // don't destroy after blinking if player collided
        if (currentPowerUp != null)
        {
            powerupRenderer.color = originalColour;
            // Debug.Log("Powerup destroyed");
            Destroy(currentPowerUp);
            SoundFXManager.Instance.PlaySoundFXClip(PowerUpDie, gameObject.transform, 1f);
        }
        blinking = false;
        idleTime = 0f;
    }
}