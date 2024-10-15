using UnityEngine;
using UnityEngine.InputSystem;

public class BotController : MonoBehaviour
{

    // Get Marble Controller from the bot when we are instantiated
    private MarbleController marble;
    private Transform marbleTransform;

    private float FLICK_COOLDOWN = 1f; // How long the bot waits between flicks
    private float flickCooldownTimer = 0; // Timer for the flick cooldown

    private float FLICK_CHARGE_TIME = 0.7f; // How long the bot charges the flick
    private float flickChargeTimer = 0; // Timer for the flick charge

    private bool isFlicking = false; // Whether the bot is currently flicking

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    // Instantiation Function
    public void SetMarble(MarbleController marble)
    {
        this.marble = marble;
        marbleTransform = marble.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (marble == null)
        {
            return;
        }

        if (flickCooldownTimer > 0)
        {
            flickCooldownTimer -= Time.deltaTime;
        }
        
        if (flickChargeTimer > 0)
        {
            flickChargeTimer -= Time.deltaTime;
            if (flickChargeTimer <= 0)
            {
                marble.ReleaseFlick();
                marble.MovementInput(Vector2.zero);
                isFlicking = false;
            }
        }

        // Recover from being knocked off the map
        if (Mathf.Abs(marbleTransform.position.x) > 9f && !isFlicking && (flickCooldownTimer <= 0 || marble.CanDash()))
        {
            float x = marbleTransform.position.x > 0 ? -1 : 1;
            float y = Mathf.Max(-marbleTransform.position.y, 0.4f);
            marble.MovementInput(new Vector2(x, y));

            if (marble.CanDash()) marble.Dash();
            else
            {
                flickChargeTimer = FLICK_CHARGE_TIME;
                isFlicking = true;
                flickCooldownTimer = FLICK_COOLDOWN;
                marble.StartChargingFlick();
            }
        }
        
    }
}
