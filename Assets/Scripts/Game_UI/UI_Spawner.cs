using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// Import the PlayerInput class from the UnityEngine.InputSystem namespace


public class UI_Spawner : MonoBehaviour
{

    [SerializeField] private StockContainer stockContainerPrefab = null;
    private Dictionary<int,StockContainer> player_containers = new Dictionary<int, StockContainer>();

    //public void spawn_ui(PlayerInfo player)
    //{
    //    foreach (StockContainer existingStockContainer in player_containers.Values)
    //    {
    //        Destroy(existingStockContainer.gameObject);
    //    }
    //    player_containers.Clear();
    //    // Now we can create the StockContainers for the players
    //    for (int i = 0; i <= playerInput.playerIndex; i++)
    //    {
    //        // Make sure the parent of the StockContainer is the parent of the PlayerJoin script
    //        StockContainer stockContainer = Instantiate(stockContainerPrefab, UI_Locations[playerInput.playerIndex+1][i], Quaternion.identity);
    //        stockContainer.setPlayerName("Player " + (i + 1));
    //        stockContainer.setPercentage(0);
    //        // Set parent to be the parent of the PlayerJoin script
    //        stockContainer.transform.SetParent(this.transform);

    //        player_containers.Add(i, stockContainer);
    //    }

    //}

    //private Dictionary<int, List<Vector3>> UI_Locations = new Dictionary<int, List<Vector3>>
    //{
    //   {1, new List<Vector3> {new Vector3(0.48f,-4.36f,0f) } },
    //   {2, new List<Vector3> {new Vector3(-5.34f, -4.42f, 0f), new Vector3(6.75f,-4.41f,0f) }},
    //   {3, new List<Vector3> {new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f), new Vector3(0,-4f,0f) }},
    //   {4, new List<Vector3> {new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f), new Vector3(0.4f,-4f,0f), new Vector3(-0.4f,-4f,0f) }}
    //};

    //public void playerLeave(PlayerInput playerInput)
    //{
    //    // Destroy the StockContainer of the player that left
    //    // Remove the StockContainer from the player_containers dictionary
    //    MarbleController player = playerInput.GetComponent<MarbleController>();
    //    StockContainer stockContainer = player_containers[playerInput.playerIndex];
    //    player_containers.Remove(playerInput.playerIndex);
    //    Destroy(stockContainer.gameObject);

    //}

}
