using UnityEngine;

public abstract class PowerupEffect : ScriptableObject
{
    public GameObject prefab;
    public float duration;

    public abstract void Apply(MarbleController target);
    public abstract void Remove(MarbleController target);
}
