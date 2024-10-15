using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
using System.Collections;
using System;
public class StockContainer : MonoBehaviour
{
    // StockContainer is assigned to each player.

    // Has a list of SingleStocks that it manages.
    // Get's the amount of SingleStocks to create from the marble's stockCount.

    [SerializeField] private Sprite cracked;
    // Text Objects
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] public int stockCount;
    [SerializeField] public float percentage;
    // dimensions of face image, initialized to start values
    private float faceImagePosX = -5.8124f;
    private float faceImagePosY = 0.51781f;
    private float faceScaleX = 3.5f;
    private float faceScaleY = 6.5f;
    [SerializeField] private Sprite stunnedFace;
    [SerializeField] private Sprite defaultFace;
    // We also get a percentage counter from the marble.

    private Coroutine animatePowerUpCoroutine;
    private bool isAnimatingPowerUpBubbles = false;

    private string name;
    private Sprite icon;

    public void setPlayerName(string name)
    {
        this.name = name;
        playerNameText.text = name;

    }

    public IEnumerator setFaceStunned(int duration)
    {
        Transform faceTransform = transform.Find("Face");
        SpriteRenderer spriteRenderer = faceTransform.GetComponent<SpriteRenderer>();
        // disable animation
        Animator animator = faceTransform.GetComponent<Animator>();
        animator.enabled = false;
        // update sprite
        spriteRenderer.sprite = stunnedFace;
        // update dimensions
        updateFaceDimensions(faceTransform, -6.8974f, 2.61f, faceScaleX, faceScaleY);
        // wait
        yield return new WaitForSeconds(duration);

        // change back to the original face
        spriteRenderer.sprite = defaultFace;
        // update dimensions
        updateFaceDimensions(faceTransform, faceImagePosX, faceImagePosY, faceScaleX, faceScaleY);
        animator.enabled = true;
    }

    public void updateFaceDimensions(Transform transform, float posX, float posY, float scaleX, float scaleY)
    {
        transform.localPosition = new Vector3(posX, posY, 0f);

        //transform.Scale = new Vector3(scaleX, scaleY, 1f);
    }

    // Also sets Mini Stock Icon
    public void setPlayerIcon(Sprite icon)
    {
        // First get a child of us called Marble_Indicator
        this.icon = icon;
        // Then get the sprite renderer of that child
        // Set the sprite of the sprite renderer to the icon
        Transform child = transform.Find("Marble_Indicator");
        if (child == null)
        {
            Debug.Log("Child not found");

        }
        if (child != null)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            // Scale the child.
            child.localScale = new Vector3(0.9f, 2f, 0.18f);
            spriteRenderer.sprite = this.icon;
        }
        setMiniStock(icon);
    }

    public void setPlayerColor(Color color)
    {
        playerNameText.color = Color.white;
        // Make the border color the same as the player's color
        playerNameText.outlineColor = color;
    }

    public void OnDamageFaceUpdate(object sender, EventArgs e)
    {
        StartCoroutine(setFaceStunned(1));
    }

    // Boolean variable to track whether the power-up animation is currently running

    public void OnPowerUpChecker(object sender, MarbleController.OnPowerUpStatus status)
    {
        if (!status.hasPowerup && isAnimatingPowerUpBubbles)
        {
            // Stop the coroutine if it's running
            if (animatePowerUpCoroutine != null)
            {
                StopCoroutine(animatePowerUpCoroutine);
                animatePowerUpCoroutine = null;
            }

            // Reset the bubble animations (make sure to clear them visually)
            StopPowerUpBubbles();
            isAnimatingPowerUpBubbles = false;
        }
    }

    public void ShowPowerUp(object sender, MarbleController.OnApplyPowerUp onPickUp)
    {
        // Start the coroutine and keep a reference to it
        animatePowerUpCoroutine = StartCoroutine(AnimatePowerUpBubbles(onPickUp.powerup.duration));

        // Apply the power-up decal sprite
        Transform powerupDecal = transform.Find("PowerUpDecal");
        SpriteRenderer spriteRenderer = powerupDecal.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = onPickUp.powerup.prefab.GetComponent<SpriteRenderer>().sprite;
    }

    private IEnumerator AnimatePowerUpBubbles(float duration)
    {
        isAnimatingPowerUpBubbles = true;  // Set the animation flag to true

        // Find the bubbles by name
        Transform mini_bubble = transform.Find("MiniPowerUpBubble");
        Transform micro_bubble = transform.Find("MicroPowerUpBubble");
        Transform power_bubble = transform.Find("PowerUpBubble");
        Transform powerup = transform.Find("PowerUpDecal");

        // Perform fading animations in sequence
        yield return StartCoroutine(FadeInSprite(micro_bubble, 0.3f));  // Fade in micro bubble
        yield return StartCoroutine(FadeInSprite(mini_bubble, 0.4f));   // Fade in mini bubble
        yield return StartCoroutine(FadeInSprite(power_bubble, 0.6f));  // Fade in power bubble
        yield return StartCoroutine(FadeInSprite(powerup, 0.1f));

        // Wait for the power-up effect to end (based on the power-up duration)
        yield return new WaitForSeconds(duration);

        // Clear all bubbles after the effect ends
        yield return StartCoroutine(FadeOutSprite(powerup, 0.1f));
        yield return StartCoroutine(FadeOutSprite(power_bubble, 0.4f));  // Fade out power bubble
        yield return StartCoroutine(FadeOutSprite(mini_bubble, 0.4f));   // Fade out mini bubble
        yield return StartCoroutine(FadeOutSprite(micro_bubble, 0.4f));  // Fade out micro bubble

        isAnimatingPowerUpBubbles = false;  // Reset the animation flag
        animatePowerUpCoroutine = null;     // Mark coroutine as finished
    }

    // A helper method to stop the bubble animations instantly
    private void StopPowerUpBubbles()
    {
        // Find the bubbles by name
        Transform mini_bubble = transform.Find("MiniPowerUpBubble");
        Transform micro_bubble = transform.Find("MicroPowerUpBubble");
        Transform power_bubble = transform.Find("PowerUpBubble");
        Transform powerup = transform.Find("PowerUpDecal");

        // Set all bubbles to invisible or clear their sprites
        SetSpriteVisibility(micro_bubble, false);
        SetSpriteVisibility(mini_bubble, false);
        SetSpriteVisibility(power_bubble, false);
        SetSpriteVisibility(powerup, false);
    }


    // Helper method to set sprite visibility (by enabling/disabling sprite renderer)
    private void SetSpriteVisibility(Transform bubble, bool visible)
    {
        if (bubble != null)
        {
            SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (!visible)
                {
                    // Set alpha to 0 (fully transparent)
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
                }
                else
                {
                    // Set alpha to 1 (fully visible)
                    spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }
    }


    // Helper coroutine for fading in the bubbles (adjusting SpriteRenderer alpha)
    private IEnumerator FadeInSprite(Transform bubble, float duration)
    {
        SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // Target color with full alpha
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);  // Interpolate the alpha value
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);  // Update color with interpolated alpha
            yield return null;
        }

        // Ensure the alpha is fully set to 1 at the end
        spriteRenderer.color = targetColor;
    }


    // Helper coroutine for fading out the bubbles (adjusting SpriteRenderer alpha)
    private IEnumerator FadeOutSprite(Transform bubble, float duration)
    {
        SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Target color with zero alpha
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);  // Interpolate the alpha value
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);  // Update color with interpolated alpha
            yield return null;
        }

        // Ensure the alpha is fully set to 0 at the end
        spriteRenderer.color = targetColor;
    }


    private void setMiniStock(Sprite icon)
    {
        // We have three children, all tagged with SingleStock
        // We must set the sprite of each child to the icon
        Transform[] children = GetComponentsInChildren<Transform>().Where(x => x.tag == "SingleStock").ToArray();
        foreach (Transform child in children)
        {
            child.localScale = new Vector3(0.35f, 0.79f, 0.24f);
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = this.icon;
        }
    }


    public void UpdateMiniStock(object sender, MarbleController.OnStockChangeArg onStock)
    {
        int stocksRemaining = onStock.stockCount;
        if (stocksRemaining <= 0)
        {
            MarbleDead();
        }
        // Get all children tagged as "SingleStock"
        Transform[] children = GetComponentsInChildren<Transform>().Where(x => x.tag == "SingleStock").ToArray();

        // Order children by the last character in their name
        children = children.OrderBy(x => x.name[x.name.Length - 1]).ToArray();



        // Iterate through the children and update their sprite based on the number of remaining stocks
        for (int i = 0; i < children.Length; i++)
        {
            SpriteRenderer spriteRenderer = children[i].GetComponent<SpriteRenderer>();

            // If this stock is "cracked" (i.e., the player has fewer stocks remaining than this index)
            if (i >= stocksRemaining && spriteRenderer != null)
            {
                spriteRenderer.sprite = cracked; // Set to cracked
                spriteRenderer.transform.localScale = new Vector3(3.72999978f, 8.41914272f, 2.55771422f);
            }
        }
    }

    private void MarbleDead()
    {
        Transform child = transform.Find("Marble_Indicator");
        if (child == null)
        {
            Debug.Log("Child not found");

        }
        SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
        // Make it gray
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
        // Set the percentage to 0
        percentage = 0;

    }





    public void setPercentage(float percentage)
    {
        this.percentage = percentage;
        percentageText.text = percentage.ToString("0.##") + "%"; // Format the percentage nicely

        // Set the color based on the percentage
        Color color = GetColorByPercentage(percentage);
        percentageText.color = color; // Apply the gradient color to the text
    }

    private Color GetColorByPercentage(float percentage)
    {
        // Clamp the percentage to a minimum of 0 but leave it open above 100 for dark red transition
        percentage = Mathf.Max(percentage, 0);

        // Define the colors for the gradient: green, yellow, red, and dark red
        Color green = Color.green;
        Color yellow = Color.yellow;
        Color red = Color.red;
        Color darkRed = new Color(0.5f, 0, 0); // Dark red for percentages above 100%

        // Interpolate between green and yellow for 0% to 50%
        if (percentage <= 50f)
        {
            return Color.Lerp(green, yellow, percentage / 50f);
        }
        // Interpolate between yellow and red for 50% to 100%
        else if (percentage <= 100f)
        {
            return Color.Lerp(yellow, red, (percentage - 50f) / 50f);
        }
        // For percentages above 100%, interpolate between red and dark red
        else
        {
            return Color.Lerp(red, darkRed, (percentage - 100f) / 100f); // The higher it goes, the darker it becomes
        }
    }






    public void Update()
    {
        // Update the stock count and percentage text
        percentageText.text = Mathf.RoundToInt(percentage).ToString() + "%";
        Color color = GetColorByPercentage(percentage);
        percentageText.color = color; // Apply the gradient color to the text

    }


    public void Start()
    {
        Transform mini_bubble = transform.Find("MiniPowerUpBubble");
        Transform micro_bubble = transform.Find("MicroPowerUpBubble");
        Transform power_bubble = transform.Find("PowerUpBubble");
        Transform powerup_decal = transform.Find("PowerUpDecal");

        // Make them all invisible, set their alpha to 0.
        mini_bubble.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        micro_bubble.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        power_bubble.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        powerup_decal.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
    }

    public void PercentageUpdater(object sender, MarbleController.OnPercentageChangeArg changeArg)
    {
        this.percentage = changeArg.percentage;
    }
}
