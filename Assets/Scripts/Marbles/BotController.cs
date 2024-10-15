using Unity.VisualScripting;
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
    private Transform targetedMarbleTransform = null; // The marble the bot is currently targeting. Null if the bot is not targeting a marble

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
                targetedMarbleTransform = null;
                marble.MovementInput(Vector2.zero);
                isFlicking = false;
            }
        }

        // Recover from being knocked off the map
        if (Mathf.Abs(marbleTransform.position.x) > 9f || marbleTransform.position.y < -5f) GetBackOntoMap();
        // Avoid other players if low on flick resource
        else if (marble.flickCounter < 3f)
        {
            Transform nearestPlayerTransform = GameManager.Instance.GetClosestPlayerTransform(marble);
            if (Vector3.Distance(nearestPlayerTransform.position, marbleTransform.position) < 2f) AvoidOtherPlayers();
            else StandStill();
        }
        // Start aiming at another marble and charging a flick
        else AttackNearerstMarble();
    }

    private void GetBackOntoMap() {
        float x = marbleTransform.position.x > 0 ? -1 : 1;
        float y = Mathf.Max(-marbleTransform.position.y, 0.4f);
        marble.MovementInput(new Vector2(x, y));

        // Dash if possible, otherwise charge a flick
        if (marble.CanDash())
        {
            marble.Dash();
            flickCooldownTimer = FLICK_COOLDOWN + Random.Range(-1f, 1f);
        }
        else if (!isFlicking)
        {
            flickChargeTimer = FLICK_CHARGE_TIME;
            isFlicking = true;
            flickCooldownTimer = FLICK_COOLDOWN + Random.Range(-1f, 1f);
            marble.StartChargingFlick();
        }
    }
    private void AvoidOtherPlayers() {
        Vector3 direction = (marbleTransform.position - GameManager.Instance.GetClosestPlayerTransform(marble).position).normalized;
        marble.MovementInput(new Vector2(direction.x, direction.y));
    }
    private void AttackNearerstMarble() {

        // Track the targeted marble
        if (targetedMarbleTransform != null)
        {
            Vector3 direction = (targetedMarbleTransform.position - marbleTransform.position).normalized;
            marble.MovementInput(new Vector2(direction.x, direction.y));
        }
        else if (!isFlicking && flickCooldownTimer <= 0)
        {
            targetedMarbleTransform = GameManager.Instance.GetClosestPlayerTransform(marble);

            flickChargeTimer = FLICK_CHARGE_TIME;
            isFlicking = true;
            flickCooldownTimer = FLICK_COOLDOWN + Random.Range(-1f, 1f);
            marble.StartChargingFlick();
        }
    }
    private void StandStill() {}

}
