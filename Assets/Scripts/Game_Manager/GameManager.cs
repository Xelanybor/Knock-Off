using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // UI Elements
    [SerializeField] private GameObject PlayerLobbyUI;
    [SerializeField] private GameObject NoPlayerLobbyUI;
    [SerializeField] private GameObject BotLobbyUI;
    [SerializeField] private GameObject StartBannerUI;
    [SerializeField] private List<Sprite> spriteList;

    // Game State
    private static bool inLobby = true;
    private static bool inGame = false;
    private static bool gameOver = false;
    private bool bannerShowing = false;
    private GameObject spawnedBanner = null;

    // Player and Bot Data
    private List<PlayerInfo> players = new List<PlayerInfo>();
    private List<BotInfo> bots = new List<BotInfo>();

    [SerializeField] private MarbleController botController;



    #region Game State Management
    public static void SetInLobby()
    {
        inLobby = true;
        inGame = false;
        gameOver = false;
    }

    public static void SetInGame()
    {
        inLobby = false;
        inGame = true;
        gameOver = false;
    }

    private void Awake()
    {
        EnsureSingleton();
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
        if (inLobby)
        {
            HandleLobbyState();
        }
        else if (inGame)
        {
            CheckForMarbleDeath();
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
        playerLobbyRect.localScale = new Vector3(120f, 120f, 1f);

        PlayerInfo playerInfo = players[playerIndex];
        UpdatePlayerUIPosition(playerInfo, playerLobbyUI.transform.position);
        UpdatePlayerLobbyUIText(playerLobbyUI, playerInfo, playerIndex);
    }

    private void CreateEmptyLobbySlot(Vector2 anchoredPosition)
    {
        GameObject noPlayerLobbyUI = Instantiate(NoPlayerLobbyUI, GameObject.FindWithTag("Canvas").transform);
        RectTransform noPlayerLobbyRect = noPlayerLobbyUI.GetComponent<RectTransform>();
        noPlayerLobbyRect.anchoredPosition = anchoredPosition;
        noPlayerLobbyRect.localScale = new Vector3(120f, 120f, 1f);
    }

    private void UpdatePlayerUIPosition(PlayerInfo playerInfo, Vector3 uiPosition)
    {
        playerInfo.playerInput.transform.position = new Vector3(uiPosition.x, uiPosition.y + 1.7f, uiPosition.z);
        playerInfo.playerInput.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        HidePlayerUIComponents(playerInfo);
    }

    private void HidePlayerUIComponents(PlayerInfo playerInfo)
    {
        MarbleController controller = playerInfo.playerInput.GetComponentInChildren<MarbleController>();
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
            playerIndex = playerInput.playerIndex
        };

        if (inLobby)
        {
            newPlayer.playerInput.SwitchCurrentActionMap("UI");
        }

        players.Add(newPlayer);
        Debug.Log($"Player {newPlayer.playerIndex} joined the game!");
    }

    private void CheckIfAllPlayersReady()
    {
        if (players.Count < 2)
        {
            return;
        }

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

    private void CheckForChangeSkin()
    {
        foreach (var player in players)
        {
            MarbleController controller = player.playerInput.GetComponentInChildren<MarbleController>();

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
        MarbleController player1 = players[0].playerInput.GetComponentInChildren<MarbleController>();
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


    public void BeginMatch()
    {
        SetInGame();
        DontDestroyPlayerObjects();
        StartCoroutine(LoadSceneAndSetup(2));
    }

    private void DontDestroyPlayerObjects()
    {
        foreach (var player in players)
        {
            DontDestroyOnLoad(player.playerInput.gameObject);
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
            player.playerInput.SwitchCurrentActionMap("Marble");
        }
    }

    private void MoveAllPlayersOffScreen()
    {
        foreach (var player in players)
        {
            player.playerInput.transform.position = new Vector3(1000, 0, 0);
            player.playerInput.transform.localScale = new Vector3(1f, 1f, 1f);
            FreezePlayerPosition(player);
        }
    }

    private void FreezePlayerPosition(PlayerInfo player)
    {
        var rb = player.playerInput.GetComponentInChildren<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        EnablePlayerUIComponents(player);
    }

    private void EnablePlayerUIComponents(PlayerInfo player)
    {
        MarbleController controller = player.playerInput.GetComponentInChildren<MarbleController>();
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
            MarbleController controller = players[i].playerInput.GetComponentInChildren<MarbleController>();
            controller.transform.position = respawnLocations[i];
            ResetPlayerPosition(players[i]);
        }
    }

    private void ResetPlayerPosition(PlayerInfo player)
    {
        var rb = player.playerInput.GetComponentInChildren<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.None;
    }
    #endregion

    #region Marble Respawn and Death
    public void CheckForMarbleDeath()
    {
        foreach (var player in players)
        {
            MarbleController mc = player.playerInput.GetComponentInChildren<MarbleController>();
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
        MarbleController marbleController = player.playerInput.GetComponentInChildren<MarbleController>();
        marbleController.transform.position = respawnLocation;

        StartCoroutine(InvincibleMarble(marbleController));
    }

    private Vector3 GetSafeRespawnLocation(List<Vector3> respawnLocations, PlayerInfo player)
    {
        Vector3 location = respawnLocations[UnityEngine.Random.Range(0, respawnLocations.Count)];

        foreach (var otherPlayer in players)
        {
            if (otherPlayer != player && Vector3.Distance(otherPlayer.playerInput.transform.position, location) < 15)
            {
                location = respawnLocations[UnityEngine.Random.Range(0, respawnLocations.Count)];
                break;
            }
        }

        return location;
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
    public int playerIndex;         // Stores the index of the player
    public Sprite playerSprite;     // Player's sprite
}

[System.Serializable]
public class BotInfo
{
    public Sprite botSprite;
}
