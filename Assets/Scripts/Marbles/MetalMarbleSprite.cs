using UnityEngine;

public class MetalMarbleSprite : MonoBehaviour
{
    
    [SerializeField] Transform parentTransform; // Reference to the parent transform
    [SerializeField] SpriteRenderer spriteRenderer; // Reference to the sprite renderer

    private bool active = false; // Boolean to check if the marble is metal

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (active) transform.rotation = Quaternion.identity;
    }

    // Function to set the marble as metal
    public void Enable()
    {
        active = true;
        spriteRenderer.enabled = true;
    }
    public void Disable()
    {
        spriteRenderer.enabled = false;
        active = false;
    }
}
