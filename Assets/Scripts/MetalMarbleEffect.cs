using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/MetalMarbleEffect")]
public class MetalMarbleEffect : PowerupEffect
{
    public float damageAmount;

    public override void Apply(GameObject target)
    {
        Debug.Log("Metal marble!");
    }
}
