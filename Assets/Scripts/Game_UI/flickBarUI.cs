using UnityEngine;
using UnityEngine.UI;
using System;
public class flickBarUI : MonoBehaviour
{
    private MarbleController player;
    [SerializeField] private Image chargeBar;       // actually the border for charge increments
    [SerializeField] private Image incrementBar;    // for incrementing the charges in intervals
    //[SerializeField] private Image contBar;         // for increm the charges over time
    [SerializeField] private Sprite[] chargeSprites;    // list of sprites for charging

    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0, 0);
    private Transform playerTransform;
    private Quaternion fixedRotation;

    public Color borderColour;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<MarbleController>();  // get the player for this UI object
        playerTransform = player.transform;

        incrementBar.fillAmount = 0.0f;
        // bind events
        player.OnCharge += Player_OnCharge;
        player.OnEnergyUpdate += Player_OnEnergyUpdate;

        chargeBar.sprite = chargeSprites[0];
        
        // Store the initial local position and rotation
        fixedRotation = Quaternion.identity;
    }

    private void Player_OnEnergyUpdate(object sender, MarbleController.OnUpdateEventArgs e)
    {
        incrementBar.fillAmount = e.progressNormalized;
    }

    void LateUpdate()
    {
        transform.rotation = fixedRotation;
        transform.position = playerTransform.position + worldOffset;
    }

    private void Player_OnCharge(object sender, MarbleController.OnFlickBarCharge e)
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
