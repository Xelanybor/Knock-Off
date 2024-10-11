using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MarbleController;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // UI Elements
    [SerializeField] private GameObject PlayerLobbyUI;
    [SerializeField] private GameObject NoPlayerLobbyUI;
    [SerializeField] private GameObject BotLobbyUI;
    [SerializeField] private GameObject StartBannerUI;
    [SerializeField] private List<Sprite> spriteList;


    private bool bannerShowing = false;
    private GameObject spawnedBanner = null;

    // Player and Bot Data
    private List<PlayerInfo> players = new List<PlayerInfo>();

    [SerializeField]
    private GameObject BotPrefab;



    #region Game State Management

    public enum GameState
    {
        MainMenu,
        Lobby,
        Game,
        Tutorial
    }

    public GameState currentState;

    private void SetGameState(GameState state)
    {
        currentState = state;
    }


    private void Awake()
    {
        EnsureSingleton();
        SceneManager.activeSceneChanged += CheckScene;
#if UNITY_EDITOR
    // Just grab the active scene.
    CheckScene(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
#endif
    }


    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject gameManagerObject = new GameObject("GameManager");
            Instance = gameManagerObject.AddComponent<GameManager>();
        }
    }


    private void EnsureSingleton()
    {
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
#endregion

    #region Game Loop
    private void Update()
    {
        switch (currentState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.Lobby:
                HandleLobbyState();
                break;
            case GameState.Game:
                CheckForMarbleDeath();
                break;
            case GameState.Tutorial:
                // Add tutorial handling logic here when necessary
                break;
        }
    }


    private void CheckScene(Scene current, Scene next)
    {
        // Determine the game state based on the next scene's name
        switch (next.name)
        {
            case "MainMenu":
                SetGameState(GameState.MainMenu);
                break;
            case "Lobby":
                GetComponent<PlayerInputManager>().enabled = true; // Enable player input
                SetGameState(GameState.Lobby);
                break;
            case "Game":
                SetGameState(GameState.Game);
                break;
            case "Tutorial":
                SetGameState(GameState.Tutorial);
                break;
            default:
                Debug.LogWarning("Unknown scene loaded: " + next.name);
                break;
        }
    }

    private void HandleLobbyState()
    {
        PaintLobbyUI();
        CheckForChangeSkin();
        CheckIfAllPlayersReady();
        StartMatch();
    }
    #endregion


    #region Tutorial Management


    #endregion

    #region Main Menu Management
    private void HandleMainMenu()
    {
        // In the main menu, disable joining.
        // We get our parent object, disable the player input manager.
        GetComponent<PlayerInputManager>().enabled = false;
    }
    #endregion


    // Event handler for new player joining
    #region Lobby Management
    private void PaintLobbyUI()
    {
        ClearExistingUI();
        GenerateLobbySlots();
    }

    private void ClearExistingUI()
    {
        foreach (Transform child in GameObject.FindWithTag("Canvas").transform)
        {
            if (child.gameObject != spawnedBanner)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void GenerateLobbySlots()
    {
        Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;

        int numberOfSlots = 4;
        float slotWidth = canvasWidth / numberOfSlots;
        float startingX = -(canvasWidth / 2) + (slotWidth / 2);

        for (int i = 0; i < numberOfSlots; i++)
        {
            Vector2 anchoredPosition = new Vector2(startingX + i * slotWidth, 0f);

            if (i < players.Count)
            {
                CreatePlayerLobbyUI(i, anchoredPosition);
            }
            else
            {
                CreateEmptyLobbySlot(anchoredPosition);
            }
        }
    }

    private void CreatePlayerLobbyUI(int playerIndex, Vector2 anchoredPosition)
    {
        GameObject playerLobbyUI = Instantiate(PlayerLobbyUI, GameObject.FindWithTag("Canvas").transform);
        RectTransform playerLobbyRect = playerLobbyUI.GetComponent<RectTransform>();
        playerLobbyRect.anchoredPosition = anchoredPosition;
        playerLobbyRect.localScale = new Vector3(300f, 300f, 1f);

        PlayerInfo playerInfo = players[playerIndex];
        if (playerInfo == null)
        {
            return;
        }
        UpdatePlayerUIPosition(playerInfo, playerLobbyUI.transform.position);
        UpdatePlayerLobbyUIText(playerLobbyUI, playerInfo, playerIndex);
    }

    private void CreateEmptyLobbySlot(Vector2 anchoredPosition)
    {
        GameObject noPlayerLobbyUI = Instantiate(NoPlayerLobbyUI, GameObject.FindWithTag("Canvas").transform);
        RectTransform noPlayerLobbyRect = noPlayerLobbyUI.GetComponent<RectTransform>();
        noPlayerLobbyRect.anchoredPosition = anchoredPosition;
        noPlayerLobbyRect.localScale = new Vector3(300f, 300f, 1f);
    }

    private void UpdatePlayerUIPosition(PlayerInfo playerInfo, Vector3 uiPosition)
    {
        if (playerInfo.parent == null)
        {
            return;
        }
        playerInfo.parent.transform.position = new Vector3(uiPosition.x, uiPosition.y + 0.2f, uiPosition.z);
        playerInfo.parent.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        HidePlayerUIComponents(playerInfo);
    }

    private void HidePlayerUIComponents(PlayerInfo playerInfo)
    {
        MarbleController controller = playerInfo.marbleController;
        controller.transform.Find("FlickBarUI").gameObject.SetActive(false);
        controller.transform.Find("PlayerMarker").gameObject.SetActive(false);
    }
    #endregion
    
    #region Player Management
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerInfo newPlayer = new PlayerInfo
        {
            playerInput = playerInput,
            parent = playerInput.gameObject,
            marbleController = playerInput.GetComponentInChildren<MarbleController>(),
            playerIndex = playerInput.playerIndex,
            AmBot = false
        };


        if (currentState == GameState.Lobby)
        {
            newPlayer.playerInput.SwitchCurrentActionMap("UI");
            newPlayer.marbleController.AddBot += Player_RequestAddBot;
            newPlayer.marbleController.RemoveBot += Player_RequestRemoveBot;
        }

        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.playerIndex} joined the game!");
    }

    // Bot Management.
    private void Player_RequestAddBot(object sender, MarbleController.OnAddBot e)
    {
        // Check if possible to add bot
        if (players.Count < 4)
        {
            PlayerInfo newPlayer = new PlayerInfo
            {
                playerInput = null,
                playerIndex = players.Count,
                playerSprite = null,
                AmBot = true
            };
            Debug.Log($"Bot {newPlayer.playerIndex} joined the game!");
            // Instantiate bot prefab
            GameObject bot = Instantiate(BotPrefab, new Vector3(0,0,0), Quaternion.identity);
            newPlayer.marbleController = bot.GetComponent<MarbleController>();
            newPlayer.parent = bot;
            newPlayer.marbleController.ready = true;
            players.Add(newPlayer);


        }
    }

    private void Player_RequestRemoveBot(object sender, MarbleController.OnRemoveBot e)
    {
        // Check if possible to remove bot
        if (players.Count < 2)
        {
            return;
        }
        foreach (var player in players)
        {
            if (player.AmBot)
            {
                players.Remove(player);
                Destroy(player.parent);
                break;
            }
        }
    }



    private void CheckIfAllPlayersReady()
    {
        if (players.Count < 2)
        {
            return;
        }

        foreach (var player in players)
        {
            if (!player.marbleController.ready)
            {
                HideBanner();
                return;
            }
        }

        ShowBanner();
    }

    private void CheckForChangeSkin()
    {
        foreach (var player in players)
        {
            MarbleController controller = player.marbleController;

            if (controller != null && controller.spriteIndex < spriteList.Count)
            {
                UpdateMarbleSprite(controller);
            }
        }
    }

    private void UpdateMarbleSprite(MarbleController controller)
    {
        Transform spriteTransform = controller.transform.Find("Sprite");
        if (spriteTransform != null)
        {
            SpriteRenderer spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteList[controller.spriteIndex];
            spriteRenderer.sortingOrder = 1;
        }
    }

    private void ShowBanner()
    {
        if (!bannerShowing)
        {
            Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
            spawnedBanner = Instantiate(StartBannerUI, canvas.transform);
            spawnedBanner.transform.position += new Vector3(0, -2f, 0);
            bannerShowing = true;
        }
    }

    private void HideBanner()
    {
        if (bannerShowing)
        {
            Destroy(spawnedBanner);
            spawnedBanner = null;
            bannerShowing = false;
        }
    }
    #endregion

    #region Game Start and Transition
    public void StartMatch()
    {
        if (players.Count < 2)
        {
            return;
        }
        MarbleController player1 = players[0].marbleController;
        if (!bannerShowing)
        {
            player1.match_can_begin = false;
            return;
        }

        
        player1.match_can_begin = true;
        if (player1.start_match)
        {
            Destroy(spawnedBanner);
            spawnedBanner = null;
            BeginMatch();
        }
    }


    private void UpdatePlayerLobbyUIText(GameObject playerLobbyUI, PlayerInfo playerInfo, int playerIndex)
    {
        MarbleController mc = playerInfo.marbleController;
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


    public void BeginMatch()
    {
        SetGameState(GameState.Game);
        DontDestroyPlayerObjects();
        StartCoroutine(LoadSceneAndSetup(2));
    }

    private void DontDestroyPlayerObjects()
    {
        foreach (var player in players)
        {
            DontDestroyOnLoad(player.parent);
        }
    }

    private IEnumerator LoadSceneAndSetup(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        SetupMatch();
    }

    private void SetupMatch()
    {
        SetControlSchemeToGame();
        MoveAllPlayersOffScreen();
        SpawnMarblesInitial();
    }

    private void SetControlSchemeToGame()
    {
        foreach (var player in players)
        {
            if (player.AmBot)
            {
                // Get the bot controller and set it up
                BotController botController = player.marbleController.GetComponent<BotController>();
                botController.SetMarble(player.marbleController);
                continue;
            }
            player.playerInput.SwitchCurrentActionMap("Marble");
        }
    }

    private void MoveAllPlayersOffScreen()
    {
        foreach (var player in players)
        {
            player.marbleController.transform.position = new Vector3(1000, 0, 0);
            player.marbleController.transform.localScale = new Vector3(1f, 1f, 1f);
            FreezePlayerPosition(player);
        }
    }

    private void FreezePlayerPosition(PlayerInfo player)
    {
        var rb = player.marbleController.GetComponentInChildren<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        EnablePlayerUIComponents(player);
    }

    private void EnablePlayerUIComponents(PlayerInfo player)
    {
        MarbleController controller = player.marbleController;
        controller.transform.Find("FlickBarUI").gameObject.SetActive(true);
        controller.transform.Find("PlayerMarker").gameObject.SetActive(true);
    }

    private void SpawnMarblesInitial()
    {
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        Map mapComponent = map.GetComponent<Map>();
        List<Vector3> respawnLocations = mapComponent.getPlayerSpawnPoints();

        for (int i = 0; i < players.Count; i++)
        {
            MarbleController controller = players[i].marbleController;
            controller.transform.position = respawnLocations[i];
            ResetPlayerPosition(players[i]);
        }
    }

    private void ResetPlayerPosition(PlayerInfo player)
    {
        var rb = player.marbleController.GetComponentInChildren<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.None;
    }
    #endregion

    #region Marble Respawn and Death
    public void CheckForMarbleDeath()
    {
        foreach (var player in players)
        {
            MarbleController mc = player.marbleController;
            if (mc.dead)
            {
                mc.dead = false;
                mc.stockCount--;
                RespawnMarble(player);
            }
        }
    }

    private void RespawnMarble(PlayerInfo player)
    {
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        Map mapComponent = map.GetComponent<Map>();
        List<Vector3> respawnLocations = mapComponent.getPlayerSpawnPoints();

        Vector3 respawnLocation = GetSafeRespawnLocation(respawnLocations, player);
        MarbleController marbleController = player.marbleController;
        marbleController.transform.position = respawnLocation;

        StartCoroutine(InvincibleMarble(marbleController));
    }

    private Vector3 GetSafeRespawnLocation(List<Vector3> respawnLocations, PlayerInfo player)
    {
        Vector3 bestLocation = Vector3.zero;
        float maxMinDistance = -1f; // Track the largest minimum distance

        // Loop through each spawn location
        foreach (var location in respawnLocations)
        {
            float minDistanceToAnyPlayer = float.MaxValue;

            // Check the distance to all other players
            foreach (var otherPlayer in players)
            {
                if (otherPlayer != player)
                {
                    float distance = Vector3.Distance(otherPlayer.marbleController.transform.position, location);
                    if (distance < minDistanceToAnyPlayer)
                    {
                        minDistanceToAnyPlayer = distance;
                    }
                }
            }

            // Track the spawn point with the largest minimum distance from any player
            if (minDistanceToAnyPlayer > maxMinDistance)
            {
                maxMinDistance = minDistanceToAnyPlayer;
                bestLocation = location;
            }
        }

        // Return the best location found
        return bestLocation;
    }

private IEnumerator InvincibleMarble(MarbleController marbleController)
    {
        Rigidbody2D rb = marbleController.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        SpriteRenderer spr = marbleController.transform.Find("Sprite").GetComponent<SpriteRenderer>();
        SpriteRenderer fire = marbleController.transform.Find("MomentumFireball").GetComponent<SpriteRenderer>();
        fire.enabled = false;

        for (int i = 0; i < 5; i++)
        {
            spr.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
            spr.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        rb.constraints = RigidbodyConstraints2D.None;
        fire.enabled = true;
    }
    #endregion
}

// Class to store each player's information
[System.Serializable]
public class PlayerInfo
{
    public PlayerInput playerInput; // Stores the PlayerInput reference
    public GameObject parent = null;
    public MarbleController marbleController; // Stores the MarbleController reference
    public int playerIndex;         // Stores the index of the player
    public Sprite playerSprite;     // Player's sprite
    public bool AmBot;              // Is the player a bot?
}

