using UnityEngine;
using System.Collections.Generic;
public class PowerupEffect : ScriptableObject
{
    public GameObject prefab;
    public float duration;
    protected Dictionary<string, float> statModifier;

    public void Apply(MarbleController target)
    {
        float value;
        if (statModifier.TryGetValue("EXTRA_FLICK", out value)) 
        {
            float diff = target.flickCounterMax - target.flickCounter;
            Debug.Log(diff);
            if (target.flickCounter < target.flickCounterMax) 
            {
                if (diff < 1)
                {
                    target.flickCounter = target.flickCounter + diff - 0.0001f;
                }
                else { target.flickCounter++; }
            }
            Debug.Log(target.flickCounter);
        } else
        {
            target.ModifyStats(statModifier);
        }
    }
    public void Remove(MarbleController target)
    {
        target.UndoStatChanges(statModifier);
    }
}
