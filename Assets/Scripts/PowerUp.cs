using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerupEffect powerupEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        powerupEffect.Apply(collision.gameObject);
        Destroy(gameObject);
    }
}
