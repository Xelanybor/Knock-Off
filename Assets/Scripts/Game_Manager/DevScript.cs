using UnityEngine;

public class DevScript : MonoBehaviour
{
    public void Start()
    {
#if UNITY_EDITOR
        // Check if GameManager already exists in the scene
        if (GameManager.Instance == null)
        {
            // Load the GameManager prefab from the Resources folder
            GameObject instance = Resources.Load<GameObject>("Game_Controller_Resource");

            if (instance != null)
            {
                // Instantiate the prefab
                Instantiate(instance);
            }
            else
            {
                Debug.LogError("Game Controller prefab not found in Resources.");
            }
        }
#endif
    }
}
