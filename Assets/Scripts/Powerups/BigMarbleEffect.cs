using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Powerups/BigMarbleEffect")]
public class BigMarbleEffect : PowerupEffect
{

    private void OnEnable()
    {
        statModifier = new Dictionary<string, float>{
            {"EXTRA_KNOCKBACK_DEALT", 0.1f},
            {"EXTRA_PERCENTAGE_DAMAGE_DEALT", 0.1f},
            {"KNOCKBACK_RESISTANCE", 0.5f},
            {"PERCENTAGE_DAMAGE_RESISTANCE", 0.2f},
        };
    }
}
