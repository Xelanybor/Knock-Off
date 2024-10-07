using UnityEngine;
using System.Collections.Generic;
public class PowerupEffect : ScriptableObject
{
    public GameObject prefab;
    public float duration;
    protected Dictionary<string, float> statModifier;

    public void Apply(MarbleController target)
    {
        target.ModifyStats(statModifier);
    }
    public void Remove(MarbleController target)
    {
        target.UndoStatChanges(statModifier);
    }
}
