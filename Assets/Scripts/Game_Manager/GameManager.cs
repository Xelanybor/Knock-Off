using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Prefab for player marble lobby UI component.
    [SerializeField]
    private GameObject PlayerLobbyUI;

    [SerializeField]
    private GameObject NoPlayerLobbyUI;

    [SerializeField]
    private GameObject BotLobbyUI;

    [SerializeField]
    private GameObject StartBannnerUI;


    private static bool in_lobby = true;
    private static bool in_game = false;
    private static bool game_over = false;

    private bool bannerShowing = false;

    private GameObject spawned_banner = null;

    [SerializeField]
    private List<Sprite> sprite_list;


    [SerializeField]
    private MarbleController botController;


    // Store players
    private List<PlayerInfo> players = new List<PlayerInfo>();

    // Store bots
    private List<BotInfo> bots = new List<BotInfo>();

    public static void SetInLobby()
    {
        in_lobby = true;
        in_game = false;
        game_over = false;
    }


    private void Awake()
    {
        // We assume we will awake in the lobby
        // Ensure singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Event handler for new player joining

    private void ShowBanner()
    {
        if (!bannerShowing)
        {
            Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
            spawned_banner = Instantiate(StartBannnerUI, canvas.transform);
            // Make sure the banner is in front of ALL UI elements
            // Move the banner down a little
            spawned_banner.transform.position = new Vector3(spawned_banner.transform.position.x, spawned_banner.transform.position.y - 2f, spawned_banner.transform.position.z);
            bannerShowing = true;
        }
    }

    private void HideBanner()
    {
        if (bannerShowing)
        {
            Destroy(spawned_banner);
            spawned_banner = null;
            bannerShowing = false;
        }

    }
    private void CheckForChangeSkin()
    {
        foreach (var player in players)
        {
            
            // Get the MarbleController from the player's input
            MarbleController marbleController = player.playerInput.GetComponentInChildren<MarbleController>();

            // Check if MarbleController exists and has a valid sprite index
            if (marbleController != null && marbleController.spriteIndex < sprite_list.Count)
            {
                // Access the Sprite child object and update its SpriteRenderer
                Transform spriteTransform = marbleController.transform.Find("Sprite");
                if (spriteTransform != null)
                {
                    SpriteRenderer spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = sprite_list[marbleController.spriteIndex];
                        spriteRenderer.sortingOrder = 1;
                    }
                }
            }
        }
    }


    private void Update()
    {
        if (in_lobby)
        {
            // Check if any player is changing their skin.
            PaintLobbyUI();
            CheckForChangeSkin();
            CheckIfAllPlayersReady();
            StartMatch();
        }
    }

    private void PaintLobbyUI()
    {
        return;
        //First, destroy any existing UI elements(this is optional but helps reset the UI)
        foreach (Transform child in GameObject.FindWithTag("Canvas").transform)
        {
            // Don't destroy the Banner if it's showing
            if (child.gameObject == spawned_banner)
            {
                // Make sure its the last sibling
                child.SetAsLastSibling();
                continue;
            }
            Destroy(child.gameObject);
        }

        // Get the Canvas and its RectTransform to calculate its size
        Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Get the width of the canvas
        float canvasWidth = canvasRect.rect.width;

        // Calculate the horizontal spacing based on the canvas width
        int numberOfSlots = 4;
        float slotWidth = canvasWidth / numberOfSlots;
        float startingX = -(canvasWidth / 2) + (slotWidth / 2); // Start from the leftmost side of the canvas

        // Create UI slots for players and empty spots
        for (int i = 0; i < numberOfSlots; i++)
        {
            // Calculate the anchored position for each slot
            Vector2 anchoredPosition = new Vector2(startingX + i * slotWidth, 0f);

            if (i < players.Count)
            {
                // Instantiate the PlayerLobbyUI prefab for each joined player
                GameObject playerLobbyUI = Instantiate(PlayerLobbyUI, canvas.transform);
                RectTransform playerLobbyRect = playerLobbyUI.GetComponent<RectTransform>();
                playerLobbyRect.anchoredPosition = anchoredPosition; // Set the UI's anchored position
                playerLobbyRect.localScale = new Vector3(300f, 300f, 1f); // Keep the UI's scale 300

                // Set the player's marble to be centered inside the PlayerLobbyUI
                PlayerInfo playerInfo = players[i];
                playerInfo.playerInput.transform.position = new Vector3(playerLobbyUI.transform.position.x, playerLobbyUI.transform.position.y + 0.2f, playerLobbyUI.transform.position.z);
                playerInfo.playerInput.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                // Update UI text for ready status
                UpdatePlayerLobbyUIText(playerLobbyUI, playerInfo, i);
            }
            else
            {
                // Instantiate NoPlayerLobbyUI prefab for empty slots
                GameObject noPlayerLobbyUI = Instantiate(NoPlayerLobbyUI, canvas.transform);
                RectTransform noPlayerLobbyRect = noPlayerLobbyUI.GetComponent<RectTransform>();
                noPlayerLobbyRect.anchoredPosition = anchoredPosition; // Set the UI's anchored position
                noPlayerLobbyRect.localScale = new Vector3(300f, 300f, 1f); ; // Keep the UI's scale 300
            }
        }
    }

    private void CheckIfAllPlayersReady()
    {
        // Return early if there are no players
        if (players.Count == 0) return;
        if (players.Count == 1)
        {
            return;
        }

            // Check if all players are ready
            foreach (var player in players)
        {
            if (!player.playerInput.GetComponent<MarbleController>().ready)
            {
                HideBanner();
                return;
                
            }
        }
        ShowBanner();
    }

    public void StartMatch()
    {
        if (!bannerShowing) return;

        // Get player 1's MarbleController
        MarbleController player1 = players[0].playerInput.GetComponentInChildren<MarbleController>();
        if (player1 == null) return;
        // Banner is showing, check if the player pressed A again.
        player1.match_can_begin = true;

        // Ensure player1 is valid and can start the match
        if (player1.start_match)
        {
            Destroy(spawned_banner);
            spawned_banner = null;
            start_match();

        }
    }

    private void UpdatePlayerLobbyUIText(GameObject playerLobbyUI, PlayerInfo playerInfo, int playerIndex)
    {
        MarbleController mc = playerInfo.playerInput.GetComponent<MarbleController>();
        TMP_Text[] labels = playerLobbyUI.GetComponentsInChildren<TMP_Text>();

        foreach (var label in labels)
        {
            if (label.name == "ReadyToolTip")
            {
                // Update the ready status
                label.text = mc.ready ? "Ready!" : "Not Ready";
            }
            else if (label.name == "ReadyStatus")
            {
                // Update the ready/unready prompt
                label.text = mc.ready ? "Press A/Space to unready." : "Press A/Space to ready up!";
            }
            if (label.name == "PlayerIndicator")
            {
                label.text = "Player " + (1 + playerIndex);
            }
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerInfo newPlayer = new PlayerInfo
        {
            playerInput = playerInput,
            playerIndex = playerInput.playerIndex,
           
        };
        // Check if we are in the lobby
        if (in_lobby)
        {
            newPlayer.playerInput.SwitchCurrentActionMap("UI");
        }

        Debug.Log("Player Joined");
        // Create a new PlayerInfo object for this player
        

        // Store the new player
        players.Add(newPlayer);

        // Optionally log or update UI
        Debug.Log($"Player {newPlayer.playerIndex} joined the game!");
        // Attempt to get the player object
        GameObject playerGameObject = playerInput.gameObject;
        PlayerInput pc = playerGameObject.GetComponentInChildren<PlayerInput>();
    }

    // Access player info if needed
    public PlayerInfo GetPlayerInfo(int playerIndex)
    {
        return players.Find(player => player.playerIndex == playerIndex);
    }

    public List<PlayerInfo> GetAllPlayers()
    {
        return players;
    }

    // Will call this when starting a match, given settings.
    public void start_match()
    {
        // Set game state to 'in game' and destroy the start banner
        in_lobby = false;
        in_game = true;
        game_over = false;

        

        // Switch to the game scene
        SceneManager.LoadSceneAsync(2);
    }

}

// Class to store each player's information
[System.Serializable]
public class PlayerInfo
{
    public PlayerInput playerInput; // Stores the PlayerInput reference
    public int playerIndex;         // Stores the index of the player
    public Sprite playerSprite;
    // Add other relevant player data as needed
}

[System.Serializable]
public class BotInfo
{
    public Sprite botSprite;
}
