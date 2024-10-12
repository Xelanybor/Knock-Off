using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// Import the PlayerInput class from the UnityEngine.InputSystem namespace


public class UI_Spawner : MonoBehaviour
{

    [SerializeField] private StockContainer stockContainerPrefab = null;
    private Dictionary<int,StockContainer> player_containers = new Dictionary<int, StockContainer>();


    public void draw_game_ui()
    {

    }


    public void spawn_ui(PlayerInfo player)
    {
        foreach (StockContainer existingStockContainer in player_containers.Values)
        {
            Destroy(existingStockContainer.gameObject);
        }
        player_containers.Clear();
        // Now we can create the StockContainers for the players
        for (int i = 0; i <= player.playerIndex; i++)
        {
            // Make sure the parent of the StockContainer is the parent of the PlayerJoin script
            StockContainer stockContainer = Instantiate(stockContainerPrefab, location, Quaternion.identity);
            stockContainer.setPlayerName("Player " + (i + 1));
            stockContainer.setPercentage(0);
            // Set parent to be the parent of the PlayerJoin script
            stockContainer.transform.SetParent(this.transform);

            player_containers.Add(i, stockContainer);
        }

    }

    public void PlayerLeave(PlayerInfo player)
    {
        // Destroy the StockContainer of the player that left
        // Remove the StockContainer from the player_containers dictionary
        StockContainer stockContainer = player_containers[player.playerIndex];
        player_containers.Remove(player.playerIndex);
        Destroy(stockContainer.gameObject);

    }

}
