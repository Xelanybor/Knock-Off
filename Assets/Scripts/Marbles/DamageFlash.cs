using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color _flashColour = Color.white;
    [SerializeField] private float _flashTime = 0.25f;

    private SpriteRenderer marbleSpriteRenderer;
    private Material material;

    private Coroutine damageFlashCoroutine;
    
    void Awake()
    {
        marbleSpriteRenderer = gameObject.transform.Find("Sprite").GetComponent<SpriteRenderer>();

        material = marbleSpriteRenderer.material;
    }

    public void CallDamageFlash()
    {
        damageFlashCoroutine = StartCoroutine(DamageFlasher());
    }

    private IEnumerator DamageFlasher()
    {
        material.SetColor("_FlashColour", _flashColour);

        float currentFlash = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < _flashTime)
        {
            elapsedTime += Time.deltaTime;

            currentFlash = Mathf.Lerp(1f, 0f, elapsedTime / _flashTime);
            material.SetFloat("_FlashAmount", currentFlash);
            yield return null;
        }
    }
}
