using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerupEffect powerupEffect;
    [SerializeField] private AudioClip powerupConsumed;

    // when a player collides with a powerup
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MarbleController player_controller = collision.gameObject.GetComponent<MarbleController>();
        if (player_controller != null && !player_controller.hasPowerup)
        {
            player_controller.ApplyPowerup(powerupEffect);
            SoundFXManager.Instance.PlaySoundFXClip(powerupConsumed, gameObject.transform, 0.5f);
            Destroy(gameObject);
        }
    }
}
