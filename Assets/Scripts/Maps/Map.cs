using UnityEngine;
using System.Collections.Generic;

public class Map : MonoBehaviour
{
    private List<Vector3> powerupSpawnPointLocations = new List<Vector3>();
    void Start()
    {
        // loop through children
        foreach (Transform child in transform)
        {
            if (child.CompareTag("PowerupSpawnPoint"))
            {
                powerupSpawnPointLocations.Add(child.position);
            }
        }
    }

    public List<Vector3> getSpawnPoints() { return powerupSpawnPointLocations; }
}
