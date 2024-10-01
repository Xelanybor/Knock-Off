using UnityEngine;

public class MomentumFireball : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    private MarbleController marbleController;
    private Transform t;
    private Transform marbleTransform;
    private Rigidbody2D marbleRb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        marbleController = GetComponentInParent<MarbleController>();
        t = GetComponent<Transform>();
        marbleTransform = marbleController.GetComponent<Transform>();
        marbleRb = marbleController.GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        SetMomentum(marbleController.GetMomentum());

        t.localRotation = Quaternion.Euler(0, 0, -marbleTransform.eulerAngles.z + Mathf.Atan2(marbleRb.linearVelocityY, marbleRb.linearVelocityX) * Mathf.Rad2Deg);

        // if (spriteRenderer.enabled) {
            // t.localEulerAngles.Set(0, 0, -marbleTransform.localEulerAngles.z);
            Debug.Log("Fireball effect rotation: " + t.localEulerAngles.z);
        // }
    }

    void SetMomentum(float momentum) {
        if (momentum > 0) {
            spriteRenderer.enabled = true;
        }
        else {
            spriteRenderer.enabled = false;
        }
    }

}
