using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Powerups/ExtraFlickEffect")]
public class ExtraFlickEffect : PowerupEffect
{
    public Dictionary<string, float> statModifier = new Dictionary<string, float>{
        {"EXTRA_PERCENTAGE_DAMAGE_DEALT", 1f}
    };
}
