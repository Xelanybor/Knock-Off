using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/MetalMarbleEffect")]
public class MetalMarbleEffect : PowerupEffect
{
    public float damageBoost;

    public override void Apply(MarbleController target)
    {
        // Debug.Log("Metal marble applied!");

        // player.damage += damageBoost;
    }

    public override void Remove(MarbleController target)
    {
        // Debug.Log("Metal marble removed!");
        // player.damage -= damageBoost;
    }
}
