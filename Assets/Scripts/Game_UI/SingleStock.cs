using UnityEngine;

public class SingleStock : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


   // SingleStock changes from the player's skin to a cracked skin when a method is called.

    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite crackedSprite;
    [SerializeField]
    private Sprite normalSprite;

    public void Crack()
    {
        spriteRenderer.sprite = crackedSprite;
    }

    public void Reset()
    {
        spriteRenderer.sprite = normalSprite;
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

}
