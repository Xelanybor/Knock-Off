using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class InputManager : MonoBehaviour
{
    [SerializeField] private Sprite[] powerupIcons;
    [SerializeField] private string[] descriptions;
    private TextMeshProUGUI powerupDescription;
    private Image powerupImage;
    private int length;         // number of choices
    private int counter;        // current choice

    private float inputCooldown = 1f;  // cooldown for updating info
    private float lastInputTime;         // last input time
    void Start()
    {
        powerupDescription = transform.Find("PowerupDescription").GetComponent<TextMeshProUGUI>();
        powerupImage = transform.Find("Polaroid").Find("powerupIcon").GetComponent<Image>();

        counter = 0;
        length = descriptions.Length;

        lastInputTime = Time.time - inputCooldown;
    }
    void Update()
    {
        float currentTime = Time.time;
        // get joystick movement direction horizontally
        float joystickHorizontal = Input.GetAxis("Horizontal");
        if (currentTime >= lastInputTime + inputCooldown)
        {
            if (joystickHorizontal < 0)
            {
                counter--;
                lastInputTime = currentTime;  // reset cooldown
            }
            else if (joystickHorizontal > 0)
            {
                counter++;
                lastInputTime = currentTime;  // reset cooldown
            }
        }
        counter = (counter + length) % length;
        powerupDescription.text = descriptions[counter];
        powerupImage.sprite = powerupIcons[counter];

        // when a pressed go back to main menu
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            StartCoroutine(wait(5f));
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
