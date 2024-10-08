using UnityEngine;
using System.Collections.Generic;

public class Map : MonoBehaviour
{

    public GameObject playerSpawnListObject;
    public GameObject powerupSpawnListObject;

    private List<Vector3> playerSpawnPointLocations = new List<Vector3>();
    private List<Vector3> powerupSpawnPointLocations = new List<Vector3>();

    void Start()
    {
        // loop through children
        foreach (Transform child in playerSpawnListObject.GetComponent<Transform>())
        {
            playerSpawnPointLocations.Add(child.position);
        }

        foreach (Transform child in powerupSpawnListObject.GetComponent<Transform>())
        {
            powerupSpawnPointLocations.Add(child.position);
        }

        Debug.Log(playerSpawnPointLocations.Count);
        Debug.Log(powerupSpawnPointLocations.Count);
    }

    public List<Vector3> getPlayerSpawnPoints() { return playerSpawnPointLocations; }
    public List<Vector3> getSpawnPoints() { return powerupSpawnPointLocations; }
}
