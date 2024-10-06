using UnityEngine;
using UnityEngine.UI;
using System;
public class flickBarUI : MonoBehaviour
{
    private MarbleController player;
    [SerializeField] private Image chargeBar;       // actually the border for charge increments
    [SerializeField] private Image incrementBar;    // for incrementing the charges over time
    [SerializeField] private Sprite[] chargeSprites;    // list of sprites for charging

    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0, 0);
    private Transform playerTransform;
    private Quaternion fixedRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<MarbleController>();  // get the player for this UI object
        playerTransform = player.transform;

        incrementBar.fillAmount = 0.0f;
        player.OnProgressCharge += Player_OnProgressCharge;
        chargeBar.sprite = chargeSprites[0];
        // Store the initial local position and rotation
        fixedRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        transform.rotation = fixedRotation;
        transform.position = playerTransform.position + worldOffset;
    }

    private void Player_OnProgressCharge(object sender, MarbleController.OnFlickBarCharge e)
    {
        chargeBar.sprite = chargeSprites[e.chargeLevel];
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void Hide() { gameObject.SetActive(false); }
    void Show() { gameObject.SetActive(true); }
}
