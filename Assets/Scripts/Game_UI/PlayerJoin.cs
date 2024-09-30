using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// Import the PlayerInput class from the UnityEngine.InputSystem namespace


public class PlayerJoin : MonoBehaviour
{
    // Listen for an event from the Player Manager.
    // When a new player joins, spawn a new StockContainer for them.
    // Manage the StockContainer lifetime, if a player leaves, destroy the StockContainer.
    // Also have a method for a AI player to join.
    // What we need from the player: Percentage, Stock Count, Player Sprite

    [SerializeField] private StockContainer stockContainerPrefab = null;
    private Dictionary<int,StockContainer> player_containers = new Dictionary<int, StockContainer>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void newPlayerJoin(PlayerInput playerInput)
    {
        // First get the player's percentage, stock count, and sprite
        // Then create a new StockContainer for the player
        // Add the StockContainer to the player_containers dictionary
        // First just get the prefab
        //MarbleController player = playerInput.
        // We can have a maximum of four players.
        // When there is only one player center the StockContainer
        // Two players, one on the left and one on the right
        // Three players, one on the left, one on the right, and one in the center
        // Four players, two on the left and two on the right

        // Let's start with one player
        // Check the player_containers dictionary to see if the player is already in it
        //if (player_containers.ContainsKey(player))
        //{
        //    Debug.LogError("Player already in the game!");
        //    return;
        //}
        // First we clear every StockContainer in the scene
        foreach (StockContainer existingStockContainer in player_containers.Values)
        {
            Debug.Log("Destroying StockContainer");
            Destroy(existingStockContainer.gameObject);
        }
        player_containers.Clear();
        // Now we can create the StockContainers for the players
        Debug.Log(playerInput.playerIndex);
        for (int i = 0; i <= playerInput.playerIndex; i++)
        {
            Debug.Log("Trying to create StockContainer");
            // Make sure the parent of the StockContainer is the parent of the PlayerJoin script
            StockContainer stockContainer = Instantiate(stockContainerPrefab, UI_Locations[playerInput.playerIndex+1][i], Quaternion.identity);
            stockContainer.setPlayerName("Player " + (i + 1));
            stockContainer.setPercentage(0);
            // Set parent to be the parent of the PlayerJoin script
            stockContainer.transform.SetParent(this.transform);

            player_containers.Add(i, stockContainer);
        }

    }

    private Dictionary<int, List<Vector3>> UI_Locations = new Dictionary<int, List<Vector3>>
    {
       {1, new List<Vector3> {new Vector3(0.48f,-4.36f,0f) } },
       {2, new List<Vector3> {new Vector3(-5.34f, -4.42f, 0f), new Vector3(6.75f,-4.41f,0f) }},
       {3, new List<Vector3> {new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f), new Vector3(0,-4f,0f) }},
       {4, new List<Vector3> {new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f), new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f) }}
    };

    public void playerLeave(PlayerInput playerInput)
    {
        // Destroy the StockContainer of the player that left
        // Remove the StockContainer from the player_containers dictionary
        MarbleController player = playerInput.GetComponent<MarbleController>();
        StockContainer stockContainer = player_containers[playerInput.playerIndex];
        player_containers.Remove(playerInput.playerIndex);
        Destroy(stockContainer.gameObject);

    }

}
