using Unity.VisualScripting;
using UnityEngine;

public class ChargeIndicator : MonoBehaviour
{

    private SpriteRenderer rend;
    [SerializeField] private Shader baseShader;
    private Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateChargeValue(0f);
    }

    public void UpdateChargeValue(float chargeValue)
    {
        if (rend == null)
        {
            material = new Material(baseShader);
            rend = GetComponent<SpriteRenderer>();
            rend.material = material;
        }
        rend.material.SetFloat("_chargeProgress", Mathf.Clamp(chargeValue, 0f, 1f));
    }
}
