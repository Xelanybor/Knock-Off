using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonSelector : MonoBehaviour
{
    public List<Button> buttonList = new List<Button>();
    private int selectedButtonIndex = 0;
    private const int NUM_BUTTONS = 3; // Play, Tutorial, Exit
    private Button activeButton;
    private float lastNavigationTime = 0f;
    private const float NAVIGATION_COOLDOWN = 0.2f; // 0.2f second cooldown

    [SerializeField] private AudioClip moveUpSound;
    [SerializeField] private AudioClip moveDownSound;
    [SerializeField] private AudioClip selectSound;

    private void Start()
    {
        if (buttonList.Count != NUM_BUTTONS)
        {
            Debug.LogError($"Expected {NUM_BUTTONS} buttons, but found {buttonList.Count}");
        }
        SelectButton(selectedButtonIndex);
    }

    public void PressSelectedButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            activeButton.onClick.Invoke();
            SoundFXManager.Instance.PlaySoundFXClip(selectSound, gameObject.transform, 0.3f);
        }
    } 

    public void MoveSelection(InputAction.CallbackContext context)
    {
        // Debug.LogError("Called Move");
        if (context.performed && Time.time - lastNavigationTime >= NAVIGATION_COOLDOWN)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            // Debug.LogError("Called Move");
            if (direction.y != 0) // Only respond to vertical input
            {
                int change = direction.y > 0 ? -1 : 1; // Up decreases index, Down increases
                selectedButtonIndex += change;
                // play sound
                if (change == 1) { SoundFXManager.Instance.PlaySoundFXClip(moveDownSound, gameObject.transform, 0.3f); }
                else { SoundFXManager.Instance.PlaySoundFXClip(moveUpSound, gameObject.transform, 0.3f);}

                // Correct wraparound logic
                if (selectedButtonIndex < 0)
                    selectedButtonIndex = NUM_BUTTONS - 1;
                else if (selectedButtonIndex >= NUM_BUTTONS)
                    selectedButtonIndex = 0;

                SelectButton(selectedButtonIndex);
                lastNavigationTime = Time.time; // Update the last navigation time
            }
        }
    }

    private void SelectButton(int index)
    {
        activeButton = buttonList[index];
        activeButton.Select();
        // Debug.Log($"Selected button: {activeButton.name} (Index: {index})");
    }
}