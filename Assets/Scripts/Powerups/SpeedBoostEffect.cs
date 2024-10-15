using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Powerups/SpeedBoostEffect")]
public class SpeedBoostEffect : PowerupEffect
{
    public void OnEnable()
    {
        statModifier = new Dictionary<string, float>{
            {"MAX_SPEED_MULTIPLIER", 1.0f },
            {"ACCELERATION_MULTIPLIER", 1.0f },
            {"JUMP_FORCE_MULTIPLIER", 0.4f }
        };
    }
}
