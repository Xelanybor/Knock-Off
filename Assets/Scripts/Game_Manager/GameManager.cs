using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static MarbleController;

struct CharacterInfo
{
    public string name;
    public List<string> buffs;
    public List<string> debuffs;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // UI Elements
    [SerializeField] private GameObject PlayerLobbyUI;
    [SerializeField] private GameObject NoPlayerLobbyUI;
    [SerializeField] private GameObject BotLobbyUI;
    [SerializeField] private GameObject StartBannerUI;
    [SerializeField] private GameObject EndScreenUI;

    [SerializeField] private List<Sprite> characterSprites;
    [SerializeField] private List<string> characterNames;



    public static Dictionary<string, int> mapVotes = new Dictionary<string, int>
{
        {"Random", 0 },
    { "Attic", 0 },
    { "Classroom", 0 },
    { "Playground", 0 }
};



    // Events
    public event EventHandler<PlayerListArg> PlayerInformationChange;
    public class PlayerListArg : EventArgs
    {
        public List<PlayerInfo> PlayerList;
    }


    private bool bannerShowing = false;
    private GameObject spawnedBanner = null;

    // Player and Bot Data
    private List<PlayerInfo> _players; // Backing field for the players list

    public ObservableCollection<PlayerInfo> players = new ObservableCollection<PlayerInfo>();


    private int botPosition = 0; // Since bots are added to the end of the list, all bots will have an index greater than this value

    private List<CharacterInfo> characterInfo = new List<CharacterInfo>
    {
        new CharacterInfo {
            name = "Cat's Eye",
            buffs = new List<string> { "Dashes further and faster" },
            debuffs = new List<string> { "Takes more knockback" }
        },
        new CharacterInfo {
            name = "Swirly",
            buffs = new List<string> { "Faster movement", "Better control" },
            debuffs = new List<string> { "Slower flick regen" }
        },
        new CharacterInfo {
            name = "Starry",
            buffs = new List<string> { "Charges flicks faster", "Faster flick regen" },
            debuffs = new List<string> { "Deals less damage" }
        },
        new CharacterInfo {
            name = "Rusty",
            buffs = new List<string> { "Takes less knockback" },
            debuffs = new List<string> { "Slower movement" }
        }
    };


    private Dictionary<PlayerInfo, Vector2> originalMarkerPositions = new Dictionary<PlayerInfo, Vector2>();
    private Dictionary<PlayerInfo, Vector2> originalPlayerNumberPositions = new Dictionary<PlayerInfo, Vector2>();

    private List<Color> playerColors = new List<Color>
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    [SerializeField]
    private GameObject BotPrefab;



    #region Game State Management

    public enum GameState
    {
        MainMenu,
        Lobby,
        Game,
        MapSelection,
        Tutorial,
        GameOver,
    }

    public GameState currentState;

    private void SetGameState(GameState state)
    {
        currentState = state;
    }

    public List<PlayerInfo> GetPlayerList()
    {
        return players.ToList();
    }


    private void Awake()
    {
        EnsureSingleton();
        players.CollectionChanged += (sender, args) =>
        {
            PlayerInformationChange?.Invoke(this, new PlayerListArg { PlayerList = players.ToList() });
        };
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
            case GameState.MapSelection:
                // Add map selection handling logic here when necessary
                HandleMapSelection();
                break;
            case GameState.Game:
                CheckForMarbleDeath();
                ZoomerMethod();
                CheckForWinCondition();
                break;
            case GameState.Tutorial:
                // Add tutorial handling logic here when necessary
                break;
        }
    }


    private void CheckScene(Scene current, Scene next)
    {
        Debug.Log("Scene changed from " + current.name + " to " + next.name);
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
            case "MapSelection":
                SetGameState(GameState.MapSelection);
                break;
            case "Arena":
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
        GameObject canvas = GameObject.FindWithTag("Canvas");
        if (canvas == null)
        {
            return;
        }
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
        if (canvas == null)
        {
            return;
        }
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;

        int numberOfSlots = 4;
        float slotWidth = canvasWidth / numberOfSlots;
        float startingX = -(canvasWidth / 2) + (slotWidth / 2);

        // Sort the players so that bots are at the end
        for (int i = 0; i < players.Count - 1; i++)
        {
            if (players[i].AmBot && !players[i+1].AmBot)
            {
                PlayerInfo temp = players[i];
                players[i] = players[i + 1];
                players[i + 1] = temp;
            }
        }

        for (int i = 0; i < numberOfSlots; i++)
        {

            // Update marble names
            if (i < players.Count)
            {
                if (players[i].AmBot) players[i].name = "CPU " + (i - botPosition + 1);
                else players[i].name = "Player " + (i + 1);
            }

            // Update player colors
            if (i < players.Count)
            {
                players[i].color = playerColors[i];
            }

            Vector2 anchoredPosition = new Vector2(startingX + i * slotWidth, 0f);

            if (i < players.Count)
            {
                if (players[i].AmBot) CreateBotLobbyUI(i, anchoredPosition);
                else CreatePlayerLobbyUI(i, anchoredPosition);
            }
            else
            {
                CreateEmptyLobbySlot(anchoredPosition);
            }
        }
    }

    private void CreatePlayerLobbyUI(int playerIndex, Vector2 anchoredPosition)
    {
        CreateMarbleLobbyUI(playerIndex, anchoredPosition, PlayerLobbyUI);
    }

    private void CreateBotLobbyUI(int playerIndex, Vector2 anchoredPosition)
    {
        CreateMarbleLobbyUI(playerIndex, anchoredPosition, BotLobbyUI);
    }

    private void CreateMarbleLobbyUI(int playerIndex, Vector2 anchoredPosition, GameObject marbleLobbyUI)
    {
        GameObject playerLobbyUI = Instantiate(marbleLobbyUI, GameObject.FindWithTag("Canvas").transform);
        RectTransform playerLobbyRect = playerLobbyUI.GetComponent<RectTransform>();
        playerLobbyRect.anchoredPosition = anchoredPosition;
        playerLobbyRect.localScale = new Vector3(120f, 120f, 1f);

        PlayerInfo playerInfo = players[playerIndex];
        if (playerInfo == null)
        {
            return;
        }
        UpdatePlayerUIPosition(playerInfo, playerLobbyUI.transform.position);
        UpdatePlayerLobbyUIText(playerLobbyUI, playerInfo, playerIndex);
        UpdatePlayerLobbyUIColour(playerLobbyUI, playerInfo);
    }

    private void CreateEmptyLobbySlot(Vector2 anchoredPosition)
    {
        GameObject noPlayerLobbyUI = Instantiate(NoPlayerLobbyUI, GameObject.FindWithTag("Canvas").transform);
        RectTransform noPlayerLobbyRect = noPlayerLobbyUI.GetComponent<RectTransform>();
        noPlayerLobbyRect.anchoredPosition = anchoredPosition;
        noPlayerLobbyRect.localScale = new Vector3(120f, 120f, 1f);
    }

    private void UpdatePlayerLobbyUIColour(GameObject playerLobbyUI, PlayerInfo playerInfo)
    {
        MarbleController mc = playerInfo.marbleController;
        SpriteRenderer[] spriteRenderers = playerLobbyUI.GetComponentsInChildren<SpriteRenderer>();

        // Which components to change the colour of
        string[] colourComponents = {
            "IndicatorChevron",
            "PlayerBorder",
        };

        foreach (var sprite in spriteRenderers)
        {
            if (colourComponents.Contains(sprite.name))
            sprite.color = playerInfo.color;
        }

        string[] colourTexts = {
            "PlayerIndicator",
            "MarbleName",
        };

        TMP_Text[] labels = playerLobbyUI.GetComponentsInChildren<TMP_Text>();

        foreach (var label in labels)
        {
            if (colourTexts.Contains(label.name))
            label.color = playerInfo.color;
        }
    }

    private void UpdatePlayerUIPosition(PlayerInfo playerInfo, Vector3 uiPosition)
    {
        if (playerInfo.parent == null)
        {
            return;
        }
        playerInfo.parent.transform.position = new Vector3(uiPosition.x, uiPosition.y + 1.7f, uiPosition.z);
        playerInfo.parent.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        HidePlayerUIComponents(playerInfo);
    }

    private void HidePlayerUIComponents(PlayerInfo playerInfo)
    {
        MarbleController controller = playerInfo.marbleController;
        controller.transform.Find("FlickBarUI").gameObject.SetActive(false);
        controller.transform.Find("PlayerMarker").gameObject.SetActive(false);
    }

    private void HidePlayerUIComponentsKeepMarker(PlayerInfo playerInfo)
    {
        MarbleController controller = playerInfo.marbleController;
        Transform markerTransform = controller.transform.Find("PlayerMarker");

        if (markerTransform != null)
        {
            markerTransform.gameObject.SetActive(true); // Ensure the marker is active

            // Get the RectTransform component
            RectTransform rectTransform = markerTransform.GetComponent<RectTransform>();
            RectTransform playerNumber = markerTransform.Find("PlayerNumber").GetComponent<RectTransform>();
            markerTransform.Find("Arrow").gameObject.SetActive(false);
            markerTransform.Find("ArrowBorder").gameObject.SetActive(false);

            if (rectTransform != null && playerNumber != null)
            {
                // Store the original anchored positions before modifying them
                if (!originalMarkerPositions.ContainsKey(playerInfo))
                {
                    originalMarkerPositions[playerInfo] = rectTransform.anchoredPosition;
                    originalPlayerNumberPositions[playerInfo] = playerNumber.anchoredPosition;
                }

                // Move the PlayerMarker DOWN by adjusting the anchoredPosition
                rectTransform.anchoredPosition += new Vector2(0, -2f); // Adjust the Y axis downwards by 2 units
                playerNumber.anchoredPosition += new Vector2(0, -2f); // Adjust the Y axis downwards by 2 units
            }
            else
            {
                Debug.LogWarning("PlayerMarker or PlayerNumber does not have a RectTransform component.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerMarker not found.");
        }
    }

    private void RestorePlayerUIComponents(PlayerInfo playerInfo)
    {
        MarbleController controller = playerInfo.marbleController;
        // Set sprite back to white
        controller.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = Color.white;
        Transform markerTransform = controller.transform.Find("PlayerMarker");

        if (markerTransform != null)
        {
            // Get the RectTransform components
            RectTransform rectTransform = markerTransform.GetComponent<RectTransform>();
            RectTransform playerNumber = markerTransform.Find("PlayerNumber").GetComponent<RectTransform>();

            if (rectTransform != null && playerNumber != null)
            {
                // Restore the original anchored positions if stored
                if (originalMarkerPositions.ContainsKey(playerInfo) && originalPlayerNumberPositions.ContainsKey(playerInfo))
                {
                    rectTransform.anchoredPosition = originalMarkerPositions[playerInfo];
                    playerNumber.anchoredPosition = originalPlayerNumberPositions[playerInfo];

                    // Optionally show the arrows again if needed
                    markerTransform.Find("Arrow").gameObject.SetActive(true);
                    markerTransform.Find("ArrowBorder").gameObject.SetActive(true);

                    // You can now remove the original positions from the dictionaries if you don't need them anymore
                    originalMarkerPositions.Remove(playerInfo);
                    originalPlayerNumberPositions.Remove(playerInfo);
                }
                else
                {
                    Debug.LogWarning("Original positions for PlayerMarker and PlayerNumber are not stored.");
                }
            }
            else
            {
                Debug.LogWarning("PlayerMarker or PlayerNumber does not have a RectTransform component.");
            }
        }
        else
        {
            Debug.LogWarning("PlayerMarker not found.");
        }
    }


    #endregion

    #region Map Selector Management

    private void HandleMapSelection()
    {
        
    }

    public void MoveMarbleOverMap(MarbleController controller, Vector3 navigationDirection)
    {
        // Ensure the game is in the map selection state
        if (currentState != GameState.MapSelection)
        {
            return;
        }
        if (controller.voted)
        {
            return;
        }

        // Get the list of map names from the mapVotes dictionary
        List<string> mapList = new List<string>(mapVotes.Keys);

        // Find the current map index of the controller's selected map
        int currentIndex = mapList.IndexOf(controller.selectedMap);

        // Determine the next or previous map based on the navigation direction
        if (navigationDirection.x > 0) // Move right to the next map
        {
            currentIndex = (currentIndex + 1) % mapList.Count; // Wrap around at the end of the list
        }
        else if (navigationDirection.x < 0) // Move left to the previous map
        {
            currentIndex = (currentIndex - 1 + mapList.Count) % mapList.Count; // Wrap around at the beginning of the list
        }

        // Decrement the vote for the previous map
        mapVotes[controller.selectedMap]--;

        // Update the selected map for the controller
        controller.selectedMap = mapList[currentIndex];

        // Increment the vote count for the newly selected map
        mapVotes[controller.selectedMap]++;

        // Re-adjust the positions of all marbles on the newly selected map
        UpdateAllMarblePositionsOnMap(controller.selectedMap);
    }

    // Helper method to update all player positions for a given map
    private void UpdateAllMarblePositionsOnMap(string mapName)
    {
        // Find the map container for the specified map
        GameObject currentOption = GameObject.Find(mapName);

        // Get the RectTransform of the current map option (assumed to be a UI element)
        RectTransform mapRectTransform = currentOption.GetComponent<RectTransform>();

        // Calculate the top-left corner of the selected map's UI element in world space
        Vector3 topLeftCorner = currentOption.transform.TransformPoint(new Vector3(-mapRectTransform.rect.width / 2, mapRectTransform.rect.height / 2, 0));

        // Define spacing between marbles for players voting for the same map
        float spacing = 0.55f; // Adjust this to control how far apart the marbles are
        float marbleScale = 0.4f; // Scale of the marbles

        // Get all players voting for this map
        var playersOnMap = players.Where(p => p.marbleController.selectedMap == mapName).ToList();
        // Remove players that are bots
        playersOnMap.RemoveAll(p => p.AmBot);

        // Iterate through the players and re-position them evenly
        for (int i = 0; i < playersOnMap.Count; i++)
        {
            var playerController = playersOnMap[i].marbleController;

            // Calculate the new position for each player based on their index
            Vector3 offset = new Vector3(0.3f, -0.5f, 0); // Adjust these values as needed
            Vector3 newPosition = topLeftCorner + new Vector3(i * spacing, 0, 0) + offset;

            // Set the marble's position and scale
            playerController.transform.position = newPosition;
            playerController.transform.localScale = Vector3.one * marbleScale;
        }
    }

    public void ConfirmVote(MarbleController controller)
    {
        if (currentState != GameState.MapSelection)
        {
            return;
        }
        
        // Lock the player, prevent them from voting again.
        controller.voted = true;
       // How do we indicate that the marble has voted?
       controller.transform.Find("Sprite").GetComponent<SpriteRenderer>().color = Color.grey;
        if (players.All(p => p.marbleController.voted))
        {
            HandleVotingEnd();
        }

    }

    public void GoBackToLobby(MarbleController controller)
    {
        if (currentState != GameState.MapSelection)
        {
            return;
        }
        // Setup the lobby again
        SetGameState(GameState.Lobby);
        // Ensure all players are set to not ready
        foreach (var player in players)
        {
            if (player.AmBot) continue;
            player.marbleController.ready = false;
        }
        SceneManager.LoadScene(1);
    }


    #endregion


    #region Player Management
    public void OnPlayerJoined(PlayerInput playerInput)
    {

        ++botPosition; // Keep track of where the bots start in the player list

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
        // Debug.Log($"Player {newPlayer.playerIndex} joined the game!");
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
            // Debug.Log($"Bot {newPlayer.playerIndex} joined the game!");
            // Instantiate bot prefab
            GameObject bot = Instantiate(BotPrefab, new Vector3(0,0,0), Quaternion.identity);
            newPlayer.marbleController = bot.GetComponent<MarbleController>();
            newPlayer.marbleController.characterIndex = UnityEngine.Random.Range(0, characterNames.Count);
            UpdateMarbleSprite(newPlayer.marbleController);
            newPlayer.parent = bot;
            newPlayer.marbleController.ready = true;
            players.Add(newPlayer);


        }
    }

    private void Player_RequestRemoveBot(object sender, MarbleController.OnRemoveBot e)
    {
        if (currentState != GameState.Lobby)
        {
            return;
        }
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

    public void ChangeMarbleCharacter(MarbleController controller, Vector3 navigationDirection)
    {
        if (currentState != GameState.Lobby)
        {
            return;
        }
        int indexChange = navigationDirection.x > 0 ? 1 : -1;
        controller.characterIndex = (controller.characterIndex + indexChange) % characterNames.Count;
        if (controller.characterIndex < 0) controller.characterIndex = characterNames.Count - 1;
        UpdateMarbleSprite(controller);
    }

    private void UpdateMarbleSprite(MarbleController controller)
    {
        Transform spriteTransform = controller.transform.Find("Sprite");
        if (spriteTransform != null)
        {
            SpriteRenderer spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = characterSprites[controller.characterIndex];
            spriteRenderer.sortingOrder = 1;
        }
    }

    private void ShowBanner()
    {
        if (!bannerShowing)
        {
            Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
            Canvas bannerCanvas = GameObject.FindWithTag("TextCanvas").GetComponent<Canvas>();
            spawnedBanner = Instantiate(StartBannerUI, bannerCanvas.transform);
            spawnedBanner.transform.position += new Vector3(0, -2f, 0);
            spawnedBanner.transform.localScale = new Vector3(120f, 120f, 1f);
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
            BeginMapVote();
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
                label.text = mc.ready ? "Press B/Esc to unready." : "Press A/Space to ready up!";
            }
            else if (label.name == "PlayerIndicator")
            {
                label.text = playerInfo.name;
            }
            else if (label.name == "MarbleName")
            {
                label.text = characterInfo[mc.characterIndex].name;
            }
            else if (label.name == "MarbleBuffs")
            {
                label.text = "▲ " + string.Join("\n▲ ", characterInfo[mc.characterIndex].buffs);
            }
            else if (label.name == "MarbleNerfs")
            {
                label.text = "▼ " + string.Join("\n▼ ", characterInfo[mc.characterIndex].debuffs);
            }
        }
    }

    // NOTE: This used to start the actual game, but now we make it load the map selection.
    public void BeginMapVote()
    {
        SetGameState(GameState.MapSelection);
        DontDestroyPlayerObjects();
        StartCoroutine(LoadSceneAndSetup(2));
    }

    public void BeginMatch()
    {
        foreach (var player in players)
        {
            RestorePlayerUIComponents(player);
        }
        SetGameState(GameState.Game);
        StartCoroutine(LoadSceneAndSetup(3));
    }

    private void DontDestroyPlayerObjects()
    {
        if (players == null) return;
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

        if (sceneIndex == 2)
        {
            SetupMapSelection();
        }

        if (sceneIndex == 3)
        {
            SetupMatch();
        }
    }


    private IEnumerator VotingCountDownTimer()
    {
        // Get the map selection UI called CountDownTimer
        GameObject countDownTimer = GameObject.Find("CountDownTimer");
        if (countDownTimer == null)
        {
            Debug.LogWarning("CountDownTimer not found.");
            yield break;
        }

        float timeLeft = 30f;
        while (timeLeft > 0)
        {
            if (players.All(p => p.marbleController.voted))
            {
                yield break;
            }
            countDownTimer.GetComponent<TMP_Text>().text = timeLeft.ToString("F0") + "s";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        HandleVotingEnd();

    }

    private void HandleVotingEnd()
    {
        if (currentState != GameState.MapSelection)
        {
            return;
        }
        Debug.Log("Voting End");
        // Get the map with the most votes, if there is a tie, select randomly, if Random is selected, select randomly.
        string selectedMap = "Random";
        int maxVotes = 0;
        List<string> tiedMaps = new List<string>();

        // Find the map with the most votes
        foreach (var map in mapVotes)
        {
            if (map.Value > maxVotes)
            {
                selectedMap = map.Key;
                maxVotes = map.Value;
                tiedMaps.Clear(); // Clear the tie list and add the current map as the only candidate
                tiedMaps.Add(map.Key);
            }
            else if (map.Value == maxVotes)
            {
                tiedMaps.Add(map.Key); // Add this map to the tie list if it has the same number of votes
            }
        }

        // Check if the selected map is "Random", if so, select a random map from the available maps
        if (selectedMap == "Random" || mapVotes["Random"] > 0)
        {
            // Select a random map
            List<string> allMaps = new List<string>(mapVotes.Keys);
            allMaps.Remove("Random"); // Remove the "Random" option from the selection
            selectedMap = allMaps[UnityEngine.Random.Range(0, allMaps.Count)];
        }
        // Check if there's a tie, if so, select randomly from the tied maps
        else if (tiedMaps.Count > 1)
        {
            selectedMap = tiedMaps[UnityEngine.Random.Range(0, tiedMaps.Count)];
        }

        // Load the selected map using PlayerPrefs
        PlayerPrefs.SetString("Map", selectedMap);

        // Proceed to load the map or perform the next step
        BeginMatch();
    }


    private void SetupMapSelection()
    {
        // Start a 30s timer for map selection
        StartCoroutine(VotingCountDownTimer());

        foreach (var player in players)
        {
            // Freeze the rigidbody
            if (player.AmBot)
            {
                // We move them off the screen and continue.
                player.marbleController.transform.position = new Vector3(1000, 0, 0);
                player.marbleController.voted = true;
                // Freeze their RB 
                FreezePlayerPosition(player);
                continue;
            }

            // Hack to select random at start.
            MoveMarbleOverMap(player.marbleController, new Vector3(0, 0, 0));
            FreezePlayerPosition(player);
            HidePlayerUIComponentsKeepMarker(player);
            SetMarbleUIColour(player);
            SetMarbleUIName(player);
        }
    }




    private void SetupMatch()
    {
        MakeMarblesReadyForGame();
        MoveAllPlayersOffScreen();
        SpawnMarblesInitial();
    }

    private void MakeMarblesReadyForGame()
    {
        foreach (var player in players)
        {
            // Set control scheme to Marble
            if (player.AmBot)
            {
                // Get the bot controller and set it up
                BotController botController = player.marbleController.GetComponent<BotController>();
                botController.SetMarble(player.marbleController);
            }
            else player.playerInput.SwitchCurrentActionMap("Marble");

            // Update marble UI
            SetMarbleUIColour(player);
            SetMarbleUIName(player);

            player.marbleController.SetMarbleType(characterNames[player.marbleController.characterIndex]);

        }
    }

    private void MoveAllPlayersOffScreen()
    {
        foreach (var player in players)
        {
            player.marbleController.transform.position = new Vector3(1000, 0, 0);
            player.marbleController.transform.localScale = new Vector3(1f, 1f, 1f);
            FreezePlayerPosition(player);
            EnablePlayerUIComponents(player);
        }
    }

    private void FreezePlayerPosition(PlayerInfo player)
    {
        var rb = player.marbleController.GetComponentInChildren<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
    }

    private void EnablePlayerUIComponents(PlayerInfo player)
    {
        MarbleController controller = player.marbleController;
        controller.transform.Find("FlickBarUI").gameObject.SetActive(true);
        controller.transform.Find("PlayerMarker").gameObject.SetActive(true);
    }

    private void SetMarbleUIColour(PlayerInfo player)
    {
        MarbleController mc = player.marbleController;
        Image[] images = mc.gameObject.GetComponentsInChildren<Image>(includeInactive: true);
        string[] colourComponents = {
            "barBorder",
            "Arrow",
        };

        foreach (var image in images)
        {
            if (colourComponents.Contains(image.name))
            image.color = player.color;
        }

        TMP_Text[] labels = mc.GetComponentsInChildren<TMP_Text>(includeInactive: true);
        string[] colourTexts = {
            "PlayerNumber"
        };

        foreach (var label in labels)
        {
            if (colourTexts.Contains(label.name))
            label.color = player.color;
        }
        
    }

    private void SetMarbleUIName(PlayerInfo player)
    {
        MarbleController mc = player.marbleController;
        TMP_Text[] labels = mc.gameObject.GetComponentsInChildren<TMP_Text>(includeInactive: true);

        foreach (var label in labels)
        {
            if (label.name == "PlayerNumber")
            label.text = player.name;
        }
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
                // Check if they have stocks, if they do, respawn them.
                if (mc.stockCount > 0)
                {
                    mc.dead = false;
                    mc.ResetPercentage();
                    RespawnMarble(player);
                }
                
            }
        }
    }

    public void CheckForWinCondition()
    {
        int remainingPlayers = 0;
        PlayerInfo winner = null;

        foreach (var player in players)
        {
            if (player.marbleController.stockCount > 0)
            {
                remainingPlayers++;
                winner = player; // Keep track of the last player with stock
            }
        }

        if (remainingPlayers == 1 && winner != null)
        {
            SetGameState(GameState.GameOver);
            foreach (var player in players)
            {
                FreezePlayerPosition(player);
                if (!player.AmBot)
                {
                    player.playerInput.SwitchCurrentActionMap("UI");
                }
                DrawEndScreen(winner);
            }
        }
    }

    private void DrawEndScreen(PlayerInfo winner)
    {
        // Display the EndScreenBanner with the winner's name
        // Instantiate the EndScreenBanner prefab
        GameObject endScreenBanner = Instantiate(EndScreenUI, new Vector3(0, 0, 0), Quaternion.identity);
        // Set the winner's name on the banner
        // Grab the WinnerText object by name
        // WinnerText is a child of TextHolder, which is a child of EndScreenBanner
        Transform winnerText = endScreenBanner.transform.Find("TextHolder/WinnerText");
        // Get the WinnerSprite object, a child of EndScreenBanner which is a SpriteRenderer
        Transform winnerSprite = endScreenBanner.transform.Find("WinnerSprite");
        // Set the winner's sprite
        winnerSprite.GetComponent<SpriteRenderer>().sprite = winner.playerSprite;
        // Get the Text component from the WinnerText object
        TMP_Text winnerTextComponent = winnerText.GetComponent<TMP_Text>();
        winnerTextComponent.text = winner.name + " wins!";
        winner.marbleController.isWinner = true;
    
   }

    public void AcceptWin(MarbleController mc)
    {
        if (currentState != GameState.GameOver) return;
        if (mc == null) return;

        // Reset the game state
        SetGameState(GameState.MainMenu);
        // Destroy the player objects.
        foreach (var player in players)
        {
            Destroy(player.parent);
        }
        players.Clear();
        // Ensure music is destroyed.
        Destroy(GameObject.Find("MusicManager"));
        // Do not destroy the game manager!
        SceneManager.LoadScene(0);
    }


    public void ZoomerMethod()
    {
        // Get the marble transforms
        Transform[] marbleTransforms = new Transform[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            marbleTransforms[i] = players[i].marbleController.transform;
        }
        // Grab the main camera from the scene, it has a tag of MainCamera
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        // Get the camera zoom controller from the scene
        CameraZoomController zoomController = mainCamera.GetComponent<CameraZoomController>();
        if (zoomController != null) zoomController.setMarbleTransforms(marbleTransforms);
    }

    private void RespawnMarble(PlayerInfo player)
    {
        if (currentState != GameState.Game) return;
        if (player == null) return;
        GameObject map = GameObject.FindGameObjectWithTag("Map");
        Map mapComponent = map.GetComponent<Map>();
        List<Vector3> respawnLocations = mapComponent.getPlayerSpawnPoints();

        Vector3 respawnLocation = GetSafeRespawnLocation(respawnLocations, player);
        MarbleController marbleController = player.marbleController;
        marbleController.transform.position = respawnLocation;

        // Reset momentum
        // marbleController.GetComponent<Rigidbody2D>().angularVelocity = 0;

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
        rb.angularVelocity = 0;
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
    public string name;             // Player's name
    public Color color;             // Player's color
}

