using UnityEngine;

public abstract class PowerupEffect : ScriptableObject
{
    public GameObject prefab;

    public abstract void Apply(GameObject target);
}
