using UnityEngine;

public class BotController : MonoBehaviour
{

    [SerializeField] private MarbleController marblePrefab;
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
        marble = Instantiate(marblePrefab, new Vector3(4, -1, 0), Quaternion.identity);
        if (!marble.TryGetComponent<Transform>(out marbleTransform))
        {
            Debug.LogError("Marble prefab does not have a Transform component!");
        }

        marble.name = "Bot Marble";
    }

    // Update is called once per frame
    void Update()
    {

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

        if (Mathf.Abs(marbleTransform.position.x) > 8.5f && !isFlicking && flickCooldownTimer <= 0)
        {
            float direction = marbleTransform.position.x > 0 ? -1 : 1;
            marble.MovementInput(new Vector2(direction, 0.4f));
            flickChargeTimer = FLICK_CHARGE_TIME;
            isFlicking = true;
            flickCooldownTimer = FLICK_COOLDOWN;
            marble.StartChargingFlick();

        }
        
    }
}
