using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Store players
    private List<PlayerInfo> players = new List<PlayerInfo>();

    private void Awake()
    {
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
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("Player Joined");
        // Create a new PlayerInfo object for this player
        PlayerInfo newPlayer = new PlayerInfo
        {
            playerInput = playerInput,
            playerIndex = playerInput.playerIndex,  // This gives the index assigned by the Player Input Manager
            // Add any other data you need here, like player name, stats, etc.
        };

        // Store the new player
        players.Add(newPlayer);

        // Optionally log or update UI
        Debug.Log($"Player {newPlayer.playerIndex} joined the game!");
        // Attempt to get the player object
        GameObject playerGameObject = playerInput.gameObject;
        PlayerInput pc = playerGameObject.GetComponentInChildren<PlayerInput>();
        pc.enabled = true;
        SpriteRenderer mc = playerGameObject.GetComponentInChildren<SpriteRenderer>();
        Debug.Log(mc.sprite.name);
        // Does this game object link to the player prefab?

        //// We can test this by printing the children of the playerGameObject
        //GameObject[] sprite = playerGameObject.GetComponentsInChildren<GameObject>();
        //if (sprite != null)
        //{
        //    for (int i = 0; i < sprite.Length; i++)
        //    {
        //        Debug.Log(sprite[i]);
        //    }
        //}
        //else
        //{
        //    Debug.Log("Player sprite not found!");
        //}
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
}

// Class to store each player's information
[System.Serializable]
public class PlayerInfo
{
    public PlayerInput playerInput; // Stores the PlayerInput reference
    public int playerIndex;         // Stores the index of the player
    // Add other relevant player data as needed
}
