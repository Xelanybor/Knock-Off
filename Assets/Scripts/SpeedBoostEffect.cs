using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/SpeedBoostEffect")]
public class SpeedBoostEffect : PowerupEffect
{
    public float speedBoost = 1.0f;

    public override void Apply(GameObject target)
    {
        Debug.Log("Speed boost!");
    }
}
