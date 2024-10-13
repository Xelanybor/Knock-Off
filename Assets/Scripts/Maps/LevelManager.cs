using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CustomArenaDict
{
    public string key;
    public GameObject value;
}


public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomArenaDict[] prefabList;
    [SerializeField] private Camera mainCamera;

    void Awake()
    {
        GameObject chosenPrefab = null;
        // get choice from playerprefs and load the correct level
        string choice = PlayerPrefs.GetString("Map");
        foreach (CustomArenaDict pair in prefabList)
        {
            if (pair.key == choice.ToUpper())
            {
                chosenPrefab = pair.value;
                break;
            }
        }
        if (chosenPrefab != null)
        {
            GameObject map = Instantiate(chosenPrefab);
            Parallax parallax = map.GetComponentInChildren<Parallax>();
            parallax.mainCamera = Camera.main;

        } else
        {
            Debug.LogError("Key for prefab level not in list!");
        }

        PlayerPrefs.DeleteKey("Map");
    }
}
