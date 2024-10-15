using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// Import the PlayerInput class from the UnityEngine.InputSystem namespace


public class UI_Spawner : MonoBehaviour
{

    [SerializeField] private StockContainer StockContainerPrefab = null;
    private Dictionary<int,StockContainer> PlayerContainers = new Dictionary<int, StockContainer>();
    List<PlayerInfo> players = new List<PlayerInfo>();


    private void Start()
    {
        // Subscribe to the GameManager's PlayerListChanged event
        GameManager.Instance.PlayerInformationChange += SpawnPlayerGameUI;
        // Draw the UI for the current players
        DrawGameUI(GameManager.Instance.GetPlayerList());
    }

    public void OnDestroy()
    {
        // Unsubscribe from the GameManager's PlayerListChanged event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerInformationChange -= SpawnPlayerGameUI;
        }
    }


    public void DrawGameUI(List<PlayerInfo> players)
    {
        // Clear previous UI elements
        foreach (StockContainer existingStockContainer in PlayerContainers.Values)
        {
            Destroy(existingStockContainer.gameObject);
        }
        PlayerContainers.Clear();

        // Get the Canvas and its RectTransform to calculate its size
        Canvas canvas = GameObject.FindWithTag("Canvas").GetComponent<Canvas>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Get the width of the canvas
        float canvasWidth = canvasRect.rect.width;

        // Calculate the number of players
        int playerCount = players.Count;
        if (playerCount == 0) return;

        // Calculate the horizontal spacing based on the number of players
        float slotWidth = canvasWidth / playerCount;
        float startingX = -(canvasWidth / 2) + (slotWidth / 2); // Start from the leftmost side of the canvas
        // Add some padding so it's a little more right
        startingX += 50f;

        // Position UI elements based on player count
        for (int i = 0; i < playerCount; i++)
        {
            // Create a new StockContainer for each player
            StockContainer stockContainer = Instantiate(StockContainerPrefab, canvas.transform);
            // Make the stock container subscribe to the player's events for percentage and power up held.
            players[i].marbleController.OnPercentageChange += stockContainer.PercentageUpdater;
            players[i].marbleController.PickUpPowerUp += stockContainer.ShowPowerUp;
            players[i].marbleController.OnStockChange += stockContainer.UpdateMiniStock;
            players[i].marbleController.OnDamageFaceUpdate += stockContainer.OnDamageFaceUpdate;
            players[i].marbleController.onPowerUpBool += stockContainer.OnPowerUpChecker;
            stockContainer.setPlayerName(players[i].name);
            stockContainer.setPercentage(players[i].marbleController.GetPercentage());
            stockContainer.setPlayerIcon(players[i].marbleController.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite);
            stockContainer.setPlayerColor(players[i].color);

            // Calculate anchored position for this player UI
            Vector2 anchoredPosition = new Vector2(startingX + i * slotWidth, canvasRect.rect.height / 2 - 100f); // Adjust height if needed
            RectTransform stockContainerRect = stockContainer.GetComponent<RectTransform>();
            stockContainerRect.anchoredPosition = anchoredPosition;
            stockContainerRect.localScale = new Vector3(22f,9.7f,73f);

            // Add to dictionary
            PlayerContainers.Add(i, stockContainer);
        }
    }

   

    public void SpawnPlayerGameUI(object sender, GameManager.PlayerListArg playerList)
    {
      if (players.Count == 0)
        {
            players = playerList.PlayerList;
            DrawGameUI(players);
        }
        // Change in player count
      if (players.Count != playerList.PlayerList.Count)
        {
            players = playerList.PlayerList;
            DrawGameUI(players);
        }
    }

    public void HandlePlayerLeaveGameUI(PlayerInfo player)
    {
        // Destroy the StockContainer of the player that left and remove from the dictionary
        if (PlayerContainers.ContainsKey(player.playerIndex))
        {
            StockContainer stockContainer = PlayerContainers[player.playerIndex];
            PlayerContainers.Remove(player.playerIndex);
            Destroy(stockContainer.gameObject);

            // Redraw the UI for remaining players
        }
    }


}
