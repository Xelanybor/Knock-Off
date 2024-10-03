using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerupEffect powerupEffect;

    // when a player collides with a powerup
    private void OnTriggerEnter2D(Collider2D collision)
    {
        MarbleController player_controller = collision.gameObject.GetComponent<MarbleController>();
        if (player_controller != null && !player_controller.hasPowerup)
        {
            player_controller.ApplyPowerup(powerupEffect);
            Destroy(gameObject);
        }
    }
}
