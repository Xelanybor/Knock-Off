using UnityEngine;

public class Parallax : MonoBehaviour
{

    public float parallaxEffectMultiplier = 0.5f;
    public Camera mainCamera;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z) * parallaxEffectMultiplier;
        // Debug.Log(ca.main.scene.path);
    }
}
