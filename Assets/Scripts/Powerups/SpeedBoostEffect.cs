using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Powerups/SpeedBoostEffect")]
public class SpeedBoostEffect : PowerupEffect
{
    public Dictionary<string, float> statModifier = new Dictionary<string, float>{
        {"MAX_SPEED_MULTIPLIER", 2f},
        {"JUMP_FORCE_MULTIPLIER", 2f }
    };
}
