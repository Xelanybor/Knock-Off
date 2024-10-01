using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/SpeedBoostEffect")]
public class SpeedBoostEffect : PowerupEffect
{
    public float speedBoost;

    public override void Apply(MarbleController target)
    {
        // Debug.Log("Speed boost applied!");
        // target.moveSpeed += speedBoost;
    }

    public override void Remove(MarbleController target)
    {
        // Debug.Log("Speed boost removed!");
        // target.moveSpeed -= speedBoost;
    }
}
