using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Powerups/BigMarbleEffect")]
public class BigMarbleEffect : PowerupEffect
{

    private void OnEnable()
    {
        statModifier = new Dictionary<string, float>{
            {"EXTRA_KNOCKBACK_DEALT", 1f},
            {"EXTRA_PERCENTAGE_DAMAGE_DEALT", 0.5f},
            {"KNOCKBACK_RESISTANCE", 1f},
            {"PERCENTAGE_DAMAGE_RESISTANCE", 0.5f},
        };
    }
}
